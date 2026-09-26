import { useEffect, useMemo, useState } from "react";
import { maintenanceApi } from "../services/maintenanceApi";

const emptyRequest = {
  propertyId: "",
  tenantId: "",
  description: "",
  category: "General",
  priority: "MEDIUM",
  imageUrl: "",
};

const emptyTechnician = {
  name: "",
  phone: "",
  email: "",
  specialization: "",
  availabilityStatus: "AVAILABLE",
};

const views = [
  { id: "home", label: "Home", icon: "01" },
  { id: "requests", label: "Maintenance requests", icon: "02" },
  { id: "technicians", label: "Technicians", icon: "03" },
  { id: "expenses", label: "Expenses", icon: "04" },
  { id: "history", label: "History", icon: "05" },
];

const unwrap = (value) =>
  Array.isArray(value) ? value : value?.items || value?.data || value?.value || [];

const statusClass = (status) => `badge status-${status?.toLowerCase().replaceAll("_", "-")}`;
const priorityClass = (priority) => `badge priority-${priority?.toLowerCase()}`;

function StakeholderDashboard() {
  const [activeView, setActiveView] = useState("home");
  const [requests, setRequests] = useState([]);
  const [technicians, setTechnicians] = useState([]);
  const [expenseRows, setExpenseRows] = useState([]);
  const [historyRows, setHistoryRows] = useState([]);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [requestHistory, setRequestHistory] = useState([]);
  const [requestExpenses, setRequestExpenses] = useState([]);
  const [aiWorkflow, setAiWorkflow] = useState(null);
  const [selectedTechnicianId, setSelectedTechnicianId] = useState(null);
  const [aiLoading, setAiLoading] = useState(false);
  const [aiComment, setAiComment] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [priorityFilter, setPriorityFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [sectionLoading, setSectionLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [showRequestForm, setShowRequestForm] = useState(false);
  const [showTechnicianForm, setShowTechnicianForm] = useState(false);
  const [editingTechnicianId, setEditingTechnicianId] = useState(null);
  const [requestForm, setRequestForm] = useState(emptyRequest);
  const [technicianForm, setTechnicianForm] = useState(emptyTechnician);

  const loadData = async () => {
    try {
      setLoading(true);
      setError("");
      const [requestData, technicianData] = await Promise.all([
        maintenanceApi.getAll(),
        maintenanceApi.getTechnicians(),
      ]);
      setRequests(unwrap(requestData));
      setTechnicians(unwrap(technicianData));
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timer = window.setTimeout(() => void loadData(), 0);
    return () => window.clearTimeout(timer);
  }, []);

  useEffect(() => {
    if (activeView !== "expenses" && activeView !== "history") return;
    const loadSectionData = async () => {
      setSectionLoading(true);
      try {
        const rows = await Promise.all(
          requests.map(async (request) => {
            const data = activeView === "expenses"
              ? await maintenanceApi.getExpenses(request.id)
              : await maintenanceApi.getHistory(request.id);
            return unwrap(data).map((item) => ({ ...item, request }));
          })
        );
        if (activeView === "expenses") setExpenseRows(rows.flat());
        else setHistoryRows(rows.flat());
      } catch (err) {
        setError(err.message);
      } finally {
        setSectionLoading(false);
      }
    };
    const timer = window.setTimeout(() => {
      if (requests.length) void loadSectionData();
      else setSectionLoading(false);
    }, 0);
    return () => window.clearTimeout(timer);
  }, [activeView, requests]);

  const filteredRequests = useMemo(() => requests.filter((request) => {
    const query = search.toLowerCase();
    return (!query || String(request.id).includes(query) || request.description?.toLowerCase().includes(query) || request.category?.toLowerCase().includes(query))
      && (!statusFilter || request.status?.toUpperCase() === statusFilter)
      && (!priorityFilter || request.priority?.toUpperCase() === priorityFilter);
  }), [requests, search, statusFilter, priorityFilter]);

  const stats = useMemo(() => ({
    total: requests.length,
    pending: requests.filter((item) => item.status?.toUpperCase() === "PENDING").length,
    progress: requests.filter((item) => ["IN_PROGRESS", "ASSIGNED", "SCHEDULED"].includes(item.status?.toUpperCase())).length,
    resolved: requests.filter((item) => item.status?.toUpperCase() === "RESOLVED").length,
    available: technicians.filter((item) => item.availabilityStatus?.toUpperCase() === "AVAILABLE").length,
    expenses: expenseRows.reduce((total, item) => total + Number(item.amount || 0), 0),
  }), [requests, technicians, expenseRows]);

  const openRequest = async (id) => {
    try {
      setError("");
      const [request, history, expenses] = await Promise.all([
        maintenanceApi.getById(id),
        maintenanceApi.getHistory(id),
        maintenanceApi.getExpenses(id),
      ]);
      setSelectedRequest(request);
      setRequestHistory(unwrap(history));
      setRequestExpenses(unwrap(expenses));
      setAiWorkflow(null);
      setSelectedTechnicianId(null);
      setAiComment("");
    } catch (err) {
      setError(err.message);
    }
  };

  const runAiWorkflow = async () => {
    if (!selectedRequest) return;
    try {
      setAiLoading(true);
      setError("");
      const result = await maintenanceApi.runAiWorkflow({
        maintenance_request_id: selectedRequest.id,
        objective: selectedRequest.description,
      });
      setAiWorkflow(result);
      setSelectedTechnicianId(result.technician_recommendation?.technician_id ?? null);
      setMessage(result.status === "PENDING_MANAGER_APPROVAL"
        ? "AI recommendation is ready for manager review."
        : `AI workflow status: ${result.status}.`);
    } catch (err) {
      setError(err.message);
    } finally {
      setAiLoading(false);
    }
  };

  const approveAiWorkflow = async (approved) => {
    if (!aiWorkflow) return;
    try {
      setAiLoading(true);
      setError("");
      const recommendation = aiWorkflow.technician_recommendation;
      const result = await maintenanceApi.approveAiWorkflow({
        workflow_id: aiWorkflow.workflow_id,
        approved_by: 1,
        approved,
        comment: aiComment,
        technician_id: selectedTechnicianId,
        scheduled_date: recommendation?.scheduled_date,
        start_time: recommendation?.start_time,
        end_time: recommendation?.end_time,
      });
      setAiWorkflow(result);
      setMessage(approved ? "Recommendation approved and sent for backend execution." : "Recommendation rejected.");
      await openRequest(selectedRequest.id);
      setAiWorkflow(result);
      setSelectedTechnicianId(result.technician_recommendation?.technician_id ?? null);
    } catch (err) {
      setError(err.message);
    } finally {
      setAiLoading(false);
    }
  };

  const createRequest = async (event) => {
    event.preventDefault();
    try {
      await maintenanceApi.create({ ...requestForm, propertyId: Number(requestForm.propertyId), tenantId: Number(requestForm.tenantId), imageUrl: requestForm.imageUrl || null });
      setMessage("Maintenance request created.");
      setRequestForm(emptyRequest);
      setShowRequestForm(false);
      await loadData();
    } catch (err) { setError(err.message); }
  };

  const saveTechnician = async (event) => {
    event.preventDefault();
    try {
      if (editingTechnicianId) await maintenanceApi.updateTechnician(editingTechnicianId, technicianForm);
      else await maintenanceApi.createTechnician(technicianForm);
      setMessage(editingTechnicianId ? "Technician updated." : "Technician added.");
      setTechnicianForm(emptyTechnician);
      setEditingTechnicianId(null);
      setShowTechnicianForm(false);
      await loadData();
    } catch (err) { setError(err.message); }
  };

  const deleteTechnician = async (id) => {
    if (!window.confirm("Delete this technician?")) return;
    try {
      await maintenanceApi.deleteTechnician(id);
      setMessage("Technician deleted.");
      await loadData();
    } catch (err) { setError(err.message); }
  };

  const renderRequestTable = (rows) => (
    <div className="table-wrapper"><table><thead><tr><th>Request</th><th>Issue</th><th>Category</th><th>Priority</th><th>Status</th><th>Created</th><th /></tr></thead><tbody>
      {rows.map((request) => <tr key={request.id}><td><strong className="request-id">#{request.id}</strong><span className="cell-subtitle">Property {request.propertyId}</span></td><td className="issue-cell">{request.description}</td><td>{request.category}</td><td><span className={priorityClass(request.priority)}>{request.priority}</span></td><td><span className={statusClass(request.status)}>{request.status}</span></td><td>{request.createdAt ? new Date(request.createdAt).toLocaleDateString() : "-"}</td><td><button className="text-button" onClick={() => openRequest(request.id)}>View details</button></td></tr>)}
      {!rows.length && <tr><td colSpan="7"><div className="empty-state">No maintenance requests match this view.</div></td></tr>}
    </tbody></table></div>
  );

  const renderHome = () => <>
    <div className="welcome-row"><div><p className="eyebrow">STAKEHOLDER OVERVIEW / 23 SEPTEMBER 2026</p><h1>Good morning, manager.</h1><p className="lede">Here is the operational pulse across your properties.</p></div><button className="primary-button" onClick={() => setShowRequestForm(true)}><span>+</span> New request</button></div>
    <div className="stats-grid dashboard-stats"><div className="stat-card stat-ink"><span>Total requests</span><strong>{stats.total}</strong><small>All maintenance activity</small></div><div className="stat-card stat-amber"><span>Pending review</span><strong>{stats.pending}</strong><small>Need your attention</small></div><div className="stat-card stat-blue"><span>In progress</span><strong>{stats.progress}</strong><small>Assigned or scheduled</small></div><div className="stat-card stat-green"><span>Technicians</span><strong>{technicians.length}</strong><small>{stats.available} currently available</small></div></div>
    <div className="home-grid"><section className="panel activity-panel"><div className="panel-heading"><div><p className="eyebrow">LIVE QUEUE</p><h2>Recent maintenance</h2></div><button className="text-button" onClick={() => setActiveView("requests")}>View all</button></div>{loading ? <div className="empty-state">Loading activity...</div> : renderRequestTable(requests.slice(0, 5))}</section><section className="panel pulse-panel"><div className="panel-heading"><div><p className="eyebrow">PORTFOLIO PULSE</p><h2>At a glance</h2></div></div><div className="pulse-list"><div><span>Resolved requests</span><strong>{stats.resolved}</strong></div><div><span>Tracked expenses</span><strong>${stats.expenses.toLocaleString(undefined, { minimumFractionDigits: 2 })}</strong></div><div><span>Technician coverage</span><strong>{technicians.length ? `${Math.round((stats.available / technicians.length) * 100)}%` : "0%"}</strong></div></div><div className="pulse-note"><span className="signal-dot" /> Data is synced with the operations database.</div></section></div>
  </>;

  const renderRequests = () => <><div className="page-title-row"><div><p className="eyebrow">OPERATIONS</p><h1>Maintenance requests</h1><p className="lede">Track every issue from first report to resolution.</p></div><button className="primary-button" onClick={() => setShowRequestForm(true)}><span>+</span> New request</button></div><section className="panel"><div className="filter-bar"><input placeholder="Search requests..." value={search} onChange={(event) => setSearch(event.target.value)} /><select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}><option value="">All statuses</option><option value="PENDING">Pending</option><option value="ASSIGNED">Assigned</option><option value="SCHEDULED">Scheduled</option><option value="IN_PROGRESS">In progress</option><option value="RESOLVED">Resolved</option><option value="REJECTED">Rejected</option></select><select value={priorityFilter} onChange={(event) => setPriorityFilter(event.target.value)}><option value="">All priorities</option><option value="LOW">Low</option><option value="MEDIUM">Medium</option><option value="HIGH">High</option></select><button className="icon-button" title="Refresh" onClick={loadData}>↻</button></div>{loading ? <div className="empty-state">Loading requests...</div> : renderRequestTable(filteredRequests)}</section></>;

  const renderTechnicians = () => <><div className="page-title-row"><div><p className="eyebrow">PEOPLE & CAPACITY</p><h1>Technicians</h1><p className="lede">Manage your maintenance team and availability.</p></div><button className="primary-button" onClick={() => { setEditingTechnicianId(null); setTechnicianForm(emptyTechnician); setShowTechnicianForm(true); }}><span>+</span> Add technician</button></div><section className="panel"><div className="section-summary"><span>{technicians.length} team members</span><span>{stats.available} available now</span></div><div className="technician-grid">{technicians.map((technician) => <article className="technician-card" key={technician.id}><div className="tech-card-top"><div className="technician-avatar">{technician.name?.charAt(0)?.toUpperCase()}</div><span className={`availability ${technician.availabilityStatus?.toLowerCase()}`}>{technician.availabilityStatus}</span></div><h3>{technician.name}</h3><p className="tech-specialization">{technician.specialization}</p><p className="tech-contact">{technician.email}<br />{technician.phone}</p><div className="card-actions"><button className="text-button" onClick={() => { setEditingTechnicianId(technician.id); setTechnicianForm({ name: technician.name || "", phone: technician.phone || "", email: technician.email || "", specialization: technician.specialization || "", availabilityStatus: technician.availabilityStatus || "AVAILABLE" }); setShowTechnicianForm(true); }}>Edit</button><button className="danger-button" onClick={() => deleteTechnician(technician.id)}>Delete</button></div></article>)}{!technicians.length && <div className="empty-state">No technicians found.</div>}</div></section></>;

  const renderExpenses = () => { const total = expenseRows.reduce((sum, row) => sum + Number(row.amount || 0), 0); return <><div className="page-title-row"><div><p className="eyebrow">FINANCE</p><h1>Expenses</h1><p className="lede">A clear view of maintenance spend by request.</p></div><div className="headline-number"><span>Total tracked</span><strong>${total.toLocaleString(undefined, { minimumFractionDigits: 2 })}</strong></div></div><section className="panel"><div className="panel-heading"><div><h2>Expense ledger</h2><p>Costs recorded against each maintenance request.</p></div></div>{sectionLoading ? <div className="empty-state">Loading expense records...</div> : <div className="table-wrapper"><table><thead><tr><th>Request</th><th>Issue</th><th>Description</th><th>Amount</th><th>Recorded</th></tr></thead><tbody>{expenseRows.map((row, index) => <tr key={row.id || `${row.request.id}-${index}`}><td><strong>#{row.request.id}</strong></td><td>{row.request.description}</td><td>{row.description || "Maintenance expense"}</td><td className="amount-cell">${Number(row.amount || 0).toFixed(2)}</td><td>{row.createdAt ? new Date(row.createdAt).toLocaleDateString() : "-"}</td></tr>)}{!expenseRows.length && <tr><td colSpan="5"><div className="empty-state">No expense records found.</div></td></tr>}</tbody></table></div>}</section></>; };

  const renderHistory = () => <><div className="page-title-row"><div><p className="eyebrow">AUDIT TRAIL</p><h1>History</h1><p className="lede">Follow the decisions and status changes behind every request.</p></div><div className="headline-number"><span>Recorded events</span><strong>{historyRows.length}</strong></div></div><section className="panel"><div className="panel-heading"><div><h2>Maintenance timeline</h2><p>Recent activity across the portfolio.</p></div></div>{sectionLoading ? <div className="empty-state">Loading history...</div> : <div className="history-list">{historyRows.map((row, index) => <div className="history-row" key={row.id || `${row.request.id}-${index}`}><div className="history-marker" /><div className="history-content"><div><strong>Request #{row.request.id}</strong><span className={statusClass(row.status || row.newStatus)}>{row.status || row.newStatus || "UPDATED"}</span></div><p>{row.comment || row.description || row.action || "Maintenance request updated."}</p><small>{row.createdAt ? new Date(row.createdAt).toLocaleString() : "Recent activity"}</small></div></div>)}{!historyRows.length && <div className="empty-state">No history records found.</div>}</div>}</section></>;

  const renderAiPanel = () => {
    if (!aiWorkflow) {
      return <section className="ai-panel"><div><p className="eyebrow">AGENTIC ASSISTANCE</p><h3>AI maintenance recommendation</h3><p className="muted">The four agents will classify this issue, find an available specialist, recommend a schedule, and validate the result. Nothing is assigned without your approval.</p></div><button className="primary-button" onClick={runAiWorkflow} disabled={aiLoading}>{aiLoading ? "Analyzing..." : "Run AI analysis"}</button></section>;
    }
    const recommendation = aiWorkflow.technician_recommendation;
    const availableTechnicians = aiWorkflow.available_technicians || [];
    return <section className="ai-panel"><div className="ai-panel-heading"><div><p className="eyebrow">AGENTIC ASSISTANCE</p><h3>AI recommendation</h3></div><span className={`ai-status ${aiWorkflow.status.toLowerCase()}`}>{aiWorkflow.status.replaceAll("_", " ")}</span></div>{aiWorkflow.analysis && <div className="ai-analysis-grid"><div><span>Category</span><strong>{aiWorkflow.analysis.category}</strong></div><div><span>Priority</span><strong>{aiWorkflow.analysis.priority}</strong></div><div><span>Specialist</span><strong>{aiWorkflow.analysis.technician_specialization}</strong></div><div><span>Duration</span><strong>{aiWorkflow.analysis.estimated_duration_minutes} min</strong></div></div>}<p className="ai-explanation">{aiWorkflow.analysis?.explanation}</p>{recommendation && <><div className="recommendation-box"><div><span className="recommendation-label">SELECTED TECHNICIAN</span><strong>{availableTechnicians.find((technician) => Number(technician.id ?? technician.technician_id) === Number(selectedTechnicianId))?.name || recommendation.technician_name}</strong><small>{recommendation.specialization}</small></div><div><span className="recommendation-label">REPAIR WINDOW</span><strong>{recommendation.scheduled_date}</strong><small>{recommendation.start_time} - {recommendation.end_time}</small></div></div>{aiWorkflow.approval_required && <div className="technician-options"><strong>Available technicians</strong>{availableTechnicians.map((technician) => { const technicianId = Number(technician.id ?? technician.technician_id); return <label key={technicianId}><input type="radio" name="ai-technician" value={technicianId} checked={Number(selectedTechnicianId) === technicianId} onChange={() => setSelectedTechnicianId(technicianId)} /><span>{technician.name || technician.technician_name}</span><small>{technician.specialization}</small></label>; })}</div>}</>}{aiWorkflow.validation && <div className={aiWorkflow.validation.is_valid ? "validation valid" : "validation invalid"}><strong>{aiWorkflow.validation.is_valid ? "Validation passed" : "Validation needs attention"}</strong>{aiWorkflow.validation.errors?.map((item) => <span key={item}>{item}</span>)}{aiWorkflow.validation.warnings?.map((item) => <span key={item}>{item}</span>)}</div>}<div className="agent-plan">{aiWorkflow.plan?.steps?.map((step) => <div key={step.step_number}><span className={`plan-dot ${step.status.toLowerCase()}`} /> <strong>{step.agent_name}</strong><small>{step.status}</small></div>)}</div>{aiWorkflow.approval_required && <><textarea placeholder="Approval note (optional)" value={aiComment} onChange={(event) => setAiComment(event.target.value)} /><div className="ai-actions"><button className="secondary-button" onClick={() => approveAiWorkflow(false)} disabled={aiLoading}>Reject recommendation</button><button className="primary-button" onClick={() => approveAiWorkflow(true)} disabled={aiLoading || !recommendation || selectedTechnicianId == null}>Approve & execute</button></div></>}</section>;
  };

  return <div className="dashboard"><aside className="sidebar"><div className="brand"><div className="brand-mark">P</div><div><strong>PropMate</strong><span>Property operations</span></div></div><nav>{views.map((view) => <button key={view.id} className={activeView === view.id ? "nav-item active" : "nav-item"} onClick={() => setActiveView(view.id)}><span className="nav-number">{view.icon}</span>{view.label}</button>)}</nav><div className="sidebar-footer"><div className="sidebar-status"><span className="signal-dot" /> System connected</div><p>Stakeholder workspace</p></div></aside><main className="main-content"><header className="topbar"><div className="breadcrumb"><span>PropMate</span><b>/</b><strong>{views.find((view) => view.id === activeView)?.label}</strong></div><div className="manager-info"><div className="avatar">PM</div><div><strong>Property Manager</strong><small>Stakeholder account</small></div></div></header><div className="content-wrap">{message && <div className="alert success">{message}<button onClick={() => setMessage("")}>×</button></div>}{error && <div className="alert error">{error}<button onClick={() => setError("")}>×</button></div>}{activeView === "home" && renderHome()}{activeView === "requests" && renderRequests()}{activeView === "technicians" && renderTechnicians()}{activeView === "expenses" && renderExpenses()}{activeView === "history" && renderHistory()}</div></main>

    {showRequestForm && <div className="modal-overlay"><div className="modal"><div className="modal-header"><div><p className="eyebrow">NEW ITEM</p><h2>Create maintenance request</h2></div><button onClick={() => setShowRequestForm(false)}>×</button></div><form onSubmit={createRequest}><label>Property ID<input type="number" required value={requestForm.propertyId} onChange={(event) => setRequestForm({ ...requestForm, propertyId: event.target.value })} /></label><label>Tenant ID<input type="number" required value={requestForm.tenantId} onChange={(event) => setRequestForm({ ...requestForm, tenantId: event.target.value })} /></label><label>Description<textarea required value={requestForm.description} onChange={(event) => setRequestForm({ ...requestForm, description: event.target.value })} /></label><div className="form-grid"><label>Category<select value={requestForm.category} onChange={(event) => setRequestForm({ ...requestForm, category: event.target.value })}><option>General</option><option>Plumbing</option><option>Electrical</option><option>HVAC</option><option>Appliance</option><option>Structural</option></select></label><label>Priority<select value={requestForm.priority} onChange={(event) => setRequestForm({ ...requestForm, priority: event.target.value })}><option>LOW</option><option>MEDIUM</option><option>HIGH</option></select></label></div><div className="modal-actions"><button type="button" className="secondary-button" onClick={() => setShowRequestForm(false)}>Cancel</button><button className="primary-button">Create request</button></div></form></div></div>}

    {showTechnicianForm && <div className="modal-overlay"><div className="modal"><div className="modal-header"><div><p className="eyebrow">TEAM DIRECTORY</p><h2>{editingTechnicianId ? "Edit technician" : "Add technician"}</h2></div><button onClick={() => setShowTechnicianForm(false)}>×</button></div><form onSubmit={saveTechnician}><label>Name<input required value={technicianForm.name} onChange={(event) => setTechnicianForm({ ...technicianForm, name: event.target.value })} /></label><div className="form-grid"><label>Phone<input required value={technicianForm.phone} onChange={(event) => setTechnicianForm({ ...technicianForm, phone: event.target.value })} /></label><label>Email<input type="email" required value={technicianForm.email} onChange={(event) => setTechnicianForm({ ...technicianForm, email: event.target.value })} /></label></div><label>Specialization<input required placeholder="e.g. Plumbing" value={technicianForm.specialization} onChange={(event) => setTechnicianForm({ ...technicianForm, specialization: event.target.value })} /></label><label>Availability<select value={technicianForm.availabilityStatus} onChange={(event) => setTechnicianForm({ ...technicianForm, availabilityStatus: event.target.value })}><option>AVAILABLE</option><option>BUSY</option><option>UNAVAILABLE</option></select></label><div className="modal-actions"><button type="button" className="secondary-button" onClick={() => setShowTechnicianForm(false)}>Cancel</button><button className="primary-button">{editingTechnicianId ? "Save changes" : "Add technician"}</button></div></form></div></div>}

    {selectedRequest && <div className="modal-overlay"><div className="modal large-modal"><div className="modal-header"><div><p className="eyebrow">REQUEST DETAIL</p><h2>Maintenance request #{selectedRequest.id}</h2></div><button onClick={() => setSelectedRequest(null)}>×</button></div><div className="detail-banner"><span className={statusClass(selectedRequest.status)}>{selectedRequest.status}</span><span className={priorityClass(selectedRequest.priority)}>{selectedRequest.priority} priority</span><strong>{selectedRequest.description}</strong></div>{renderAiPanel()}<div className="detail-grid"><div><h3>Request information</h3><div className="detail-row"><span>Property</span><strong>{selectedRequest.propertyId}</strong></div><div className="detail-row"><span>Tenant</span><strong>{selectedRequest.tenantId}</strong></div><div className="detail-row"><span>Category</span><strong>{selectedRequest.category}</strong></div></div><div><h3>Expense records</h3>{requestExpenses.length ? requestExpenses.map((expense) => <div className="mini-row" key={expense.id}><span>{expense.description}</span><strong>${Number(expense.amount || 0).toFixed(2)}</strong></div>) : <p className="muted">No expenses recorded.</p>}</div></div><div className="detail-history"><h3>Recent history</h3>{requestHistory.length ? requestHistory.map((event, index) => <div className="mini-history" key={event.id || index}><span className="history-marker" /><div><strong>{event.status || event.newStatus || "Updated"}</strong><p>{event.comment || event.description || event.action || "Request updated."}</p></div></div>) : <p className="muted">No history recorded.</p>}</div></div></div>}
  </div>;
}

export default StakeholderDashboard;
