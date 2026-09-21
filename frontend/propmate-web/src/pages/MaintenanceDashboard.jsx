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

function MaintenanceDashboard() {
  const [requests, setRequests] = useState([]);
  const [technicians, setTechnicians] = useState([]);

  const [selectedRequest, setSelectedRequest] = useState(null);
  const [history, setHistory] = useState([]);

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [priorityFilter, setPriorityFilter] = useState("");

  const [activeTab, setActiveTab] = useState("requests");

  const [showRequestForm, setShowRequestForm] = useState(false);
  const [showTechnicianForm, setShowTechnicianForm] = useState(false);

  const [requestForm, setRequestForm] = useState(emptyRequest);
  const [technicianForm, setTechnicianForm] = useState(emptyTechnician);

  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const [scheduleForm, setScheduleForm] = useState({
    technicianId: "",
    scheduledDate: "",
    startTime: "",
    endTime: "",
    notes: "",
  });

  const [expenseForm, setExpenseForm] = useState({
    amount: "",
    description: "",
    recordedBy: 1,
  });

  const loadData = async () => {
    try {
      setLoading(true);
      setError("");

      const [maintenanceData, technicianData] = await Promise.all([
        maintenanceApi.getAll(),
        maintenanceApi.getTechnicians(),
      ]);

      setRequests(
        Array.isArray(maintenanceData)
          ? maintenanceData
          : maintenanceData.items || maintenanceData.data || []
      );

      setTechnicians(
        Array.isArray(technicianData)
          ? technicianData
          : technicianData.items || technicianData.data || []
      );
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const filteredRequests = useMemo(() => {
    return requests.filter((request) => {
      const searchText = search.toLowerCase();

      const matchesSearch =
        !search ||
        String(request.id).includes(searchText) ||
        request.description?.toLowerCase().includes(searchText) ||
        request.category?.toLowerCase().includes(searchText);

      const matchesStatus =
        !statusFilter ||
        request.status?.toUpperCase() === statusFilter;

      const matchesPriority =
        !priorityFilter ||
        request.priority?.toUpperCase() === priorityFilter;

      return matchesSearch && matchesStatus && matchesPriority;
    });
  }, [requests, search, statusFilter, priorityFilter]);

  const openRequest = async (id) => {
    try {
      setError("");

      const data = await maintenanceApi.getById(id);
      const historyData = await maintenanceApi.getHistory(id);

      setSelectedRequest(data);

      setHistory(
        Array.isArray(historyData)
          ? historyData
          : historyData.items || historyData.data || []
      );
    } catch (err) {
      setError(err.message);
    }
  };

  const createRequest = async (event) => {
    event.preventDefault();

    try {
      await maintenanceApi.create({
        propertyId: Number(requestForm.propertyId),
        tenantId: Number(requestForm.tenantId),
        description: requestForm.description,
        category: requestForm.category,
        priority: requestForm.priority,
        imageUrl: requestForm.imageUrl || null,
      });

      setMessage("Maintenance request created successfully.");
      setRequestForm(emptyRequest);
      setShowRequestForm(false);

      await loadData();
    } catch (err) {
      setError(err.message);
    }
  };

  const assignTechnician = async (technicianId) => {
    if (!selectedRequest) return;

    try {
      await maintenanceApi.assign(selectedRequest.id, {
        technicianId: Number(technicianId),
        assignedBy: 1,
        notes: "Assigned by property manager.",
      });

      setMessage("Technician assigned successfully.");
      await openRequest(selectedRequest.id);
      await loadData();
    } catch (err) {
      setError(err.message);
    }
  };

  const scheduleRepair = async (event) => {
    event.preventDefault();

    if (!selectedRequest) return;

    try {
      await maintenanceApi.schedule(selectedRequest.id, {
        technicianId: Number(scheduleForm.technicianId),
        scheduledDate: scheduleForm.scheduledDate,
        startTime: scheduleForm.startTime,
        endTime: scheduleForm.endTime,
        notes: scheduleForm.notes,
      });

      setMessage("Repair scheduled successfully.");

      setScheduleForm({
        technicianId: "",
        scheduledDate: "",
        startTime: "",
        endTime: "",
        notes: "",
      });

      await openRequest(selectedRequest.id);
      await loadData();
    } catch (err) {
      setError(err.message);
    }
  };

  const changeStatus = async (newStatus) => {
    if (!selectedRequest) return;

    try {
      await maintenanceApi.updateStatus(selectedRequest.id, {
        status: newStatus,
        changedBy: 1,
        comment: `Status changed to ${newStatus}.`,
      });

      setMessage("Status updated successfully.");

      await openRequest(selectedRequest.id);
      await loadData();
    } catch (err) {
      setError(err.message);
    }
  };

  const addExpense = async (event) => {
    event.preventDefault();

    if (!selectedRequest) return;

    try {
      await maintenanceApi.addExpense(selectedRequest.id, {
        amount: Number(expenseForm.amount),
        description: expenseForm.description,
        recordedBy: Number(expenseForm.recordedBy),
      });

      setMessage("Expense added successfully.");

      setExpenseForm({
        amount: "",
        description: "",
        recordedBy: 1,
      });

      await openRequest(selectedRequest.id);
    } catch (err) {
      setError(err.message);
    }
  };

  const createTechnician = async (event) => {
    event.preventDefault();

    try {
      await maintenanceApi.createTechnician(technicianForm);

      setMessage("Technician created successfully.");
      setTechnicianForm(emptyTechnician);
      setShowTechnicianForm(false);

      await loadData();
    } catch (err) {
      setError(err.message);
    }
  };

  const deleteTechnician = async (id) => {
    if (!window.confirm("Delete this technician?")) return;

    try {
      await maintenanceApi.deleteTechnician(id);

      setMessage("Technician deleted successfully.");
      await loadData();
    } catch (err) {
      setError(err.message);
    }
  };

  const statusClass = (status) =>
    `badge status-${status?.toLowerCase().replace("_", "-")}`;

  const priorityClass = (priority) =>
    `badge priority-${priority?.toLowerCase()}`;

  return (
    <div className="dashboard">

      <header className="topbar">
        <div>
          <h1>PropMate</h1>
          <span>Property Management System</span>
        </div>

        <div className="manager-info">
          <div className="avatar">PM</div>
          <div>
            <strong>Property Manager</strong>
            <small>Maintenance Management</small>
          </div>
        </div>
      </header>

      <main className="main-content">

        <section className="page-heading">
          <div>
            <p className="eyebrow">PROPERTY OPERATIONS</p>
            <h2>Maintenance Management</h2>
            <p>
              Manage maintenance requests, technicians, repairs and expenses.
            </p>
          </div>

          <button
            className="primary-button"
            onClick={() => setShowRequestForm(true)}
          >
            + New Request
          </button>
        </section>

        {message && (
          <div className="alert success">
            {message}
            <button onClick={() => setMessage("")}>×</button>
          </div>
        )}

        {error && (
          <div className="alert error">
            {error}
            <button onClick={() => setError("")}>×</button>
          </div>
        )}

        <div className="stats-grid">
          <div className="stat-card">
            <span>Total Requests</span>
            <strong>{requests.length}</strong>
          </div>

          <div className="stat-card">
            <span>Pending</span>
            <strong>
              {
                requests.filter(
                  (r) => r.status?.toUpperCase() === "PENDING"
                ).length
              }
            </strong>
          </div>

          <div className="stat-card">
            <span>In Progress</span>
            <strong>
              {
                requests.filter(
                  (r) => r.status?.toUpperCase() === "IN_PROGRESS"
                ).length
              }
            </strong>
          </div>

          <div className="stat-card">
            <span>Technicians</span>
            <strong>{technicians.length}</strong>
          </div>
        </div>

        <div className="tabs">
          <button
            className={activeTab === "requests" ? "active" : ""}
            onClick={() => setActiveTab("requests")}
          >
            Maintenance Requests
          </button>

          <button
            className={activeTab === "technicians" ? "active" : ""}
            onClick={() => setActiveTab("technicians")}
          >
            Technicians
          </button>
        </div>

        {activeTab === "requests" && (
          <section className="content-card">

            <div className="filter-bar">
              <input
                placeholder="Search by ID, description or category..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />

              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <option value="">All Statuses</option>
                <option value="PENDING">Pending</option>
                <option value="ASSIGNED">Assigned</option>
                <option value="SCHEDULED">Scheduled</option>
                <option value="IN_PROGRESS">In Progress</option>
                <option value="RESOLVED">Resolved</option>
                <option value="REJECTED">Rejected</option>
                <option value="CANCELLED">Cancelled</option>
              </select>

              <select
                value={priorityFilter}
                onChange={(e) => setPriorityFilter(e.target.value)}
              >
                <option value="">All Priorities</option>
                <option value="LOW">Low</option>
                <option value="MEDIUM">Medium</option>
                <option value="HIGH">High</option>
              </select>

              <button className="secondary-button" onClick={loadData}>
                Refresh
              </button>
            </div>

            {loading ? (
              <div className="empty-state">Loading requests...</div>
            ) : (
              <div className="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>Issue</th>
                      <th>Category</th>
                      <th>Priority</th>
                      <th>Status</th>
                      <th>Created</th>
                      <th></th>
                    </tr>
                  </thead>

                  <tbody>
                    {filteredRequests.map((request) => (
                      <tr key={request.id}>
                        <td>#{request.id}</td>

                        <td>
                          <strong>{request.description}</strong>
                        </td>

                        <td>{request.category}</td>

                        <td>
                          <span className={priorityClass(request.priority)}>
                            {request.priority}
                          </span>
                        </td>

                        <td>
                          <span className={statusClass(request.status)}>
                            {request.status}
                          </span>
                        </td>

                        <td>
                          {request.createdAt
                            ? new Date(
                                request.createdAt
                              ).toLocaleDateString()
                            : "-"}
                        </td>

                        <td>
                          <button
                            className="view-button"
                            onClick={() => openRequest(request.id)}
                          >
                            View
                          </button>
                        </td>
                      </tr>
                    ))}

                    {filteredRequests.length === 0 && (
                      <tr>
                        <td colSpan="7">
                          <div className="empty-state">
                            No maintenance requests found.
                          </div>
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        )}

        {activeTab === "technicians" && (
          <section className="content-card">

            <div className="section-header">
              <div>
                <h3>Technicians</h3>
                <p>Manage maintenance technicians and availability.</p>
              </div>

              <button
                className="primary-button"
                onClick={() => setShowTechnicianForm(true)}
              >
                + Add Technician
              </button>
            </div>

            <div className="technician-grid">
              {technicians.map((technician) => (
                <div className="technician-card" key={technician.id}>
                  <div className="technician-avatar">
                    {technician.name?.charAt(0)?.toUpperCase()}
                  </div>

                  <div className="technician-info">
                    <h3>{technician.name}</h3>
                    <p>{technician.specialization}</p>
                    <small>{technician.phone}</small>
                    <small>{technician.email}</small>

                    <span
                      className={`availability ${
                        technician.availabilityStatus?.toLowerCase()
                      }`}
                    >
                      {technician.availabilityStatus}
                    </span>
                  </div>

                  <button
                    className="delete-button"
                    onClick={() => deleteTechnician(technician.id)}
                  >
                    Delete
                  </button>
                </div>
              ))}

              {technicians.length === 0 && (
                <div className="empty-state">
                  No technicians available.
                </div>
              )}
            </div>
          </section>
        )}

      </main>

      {showRequestForm && (
        <div className="modal-overlay">
          <div className="modal">
            <div className="modal-header">
              <div>
                <h2>New Maintenance Request</h2>
                <p>Create a maintenance request.</p>
              </div>

              <button onClick={() => setShowRequestForm(false)}>×</button>
            </div>

            <form onSubmit={createRequest}>

              <label>Property ID</label>
              <input
                type="number"
                required
                value={requestForm.propertyId}
                onChange={(e) =>
                  setRequestForm({
                    ...requestForm,
                    propertyId: e.target.value,
                  })
                }
              />

              <label>Tenant ID</label>
              <input
                type="number"
                required
                value={requestForm.tenantId}
                onChange={(e) =>
                  setRequestForm({
                    ...requestForm,
                    tenantId: e.target.value,
                  })
                }
              />

              <label>Description</label>
              <textarea
                required
                value={requestForm.description}
                onChange={(e) =>
                  setRequestForm({
                    ...requestForm,
                    description: e.target.value,
                  })
                }
              />

              <label>Category</label>
              <select
                value={requestForm.category}
                onChange={(e) =>
                  setRequestForm({
                    ...requestForm,
                    category: e.target.value,
                  })
                }
              >
                <option>General</option>
                <option>Plumbing</option>
                <option>Electrical</option>
                <option>HVAC</option>
                <option>Appliance</option>
                <option>Structural</option>
                <option>Other</option>
              </select>

              <label>Priority</label>
              <select
                value={requestForm.priority}
                onChange={(e) =>
                  setRequestForm({
                    ...requestForm,
                    priority: e.target.value,
                  })
                }
              >
                <option>LOW</option>
                <option>MEDIUM</option>
                <option>HIGH</option>
              </select>

              <label>Image URL (optional)</label>
              <input
                type="text"
                value={requestForm.imageUrl}
                onChange={(e) =>
                  setRequestForm({
                    ...requestForm,
                    imageUrl: e.target.value,
                  })
                }
              />

              <div className="modal-actions">
                <button
                  type="button"
                  className="secondary-button"
                  onClick={() => setShowRequestForm(false)}
                >
                  Cancel
                </button>

                <button className="primary-button" type="submit">
                  Create Request
                </button>
              </div>

            </form>
          </div>
        </div>
      )}

      {showTechnicianForm && (
        <div className="modal-overlay">
          <div className="modal">

            <div className="modal-header">
              <div>
                <h2>Add Technician</h2>
                <p>Add a new maintenance technician.</p>
              </div>

              <button onClick={() => setShowTechnicianForm(false)}>
                ×
              </button>
            </div>

            <form onSubmit={createTechnician}>

              <label>Name</label>
              <input
                required
                value={technicianForm.name}
                onChange={(e) =>
                  setTechnicianForm({
                    ...technicianForm,
                    name: e.target.value,
                  })
                }
              />

              <label>Phone</label>
              <input
                required
                value={technicianForm.phone}
                onChange={(e) =>
                  setTechnicianForm({
                    ...technicianForm,
                    phone: e.target.value,
                  })
                }
              />

              <label>Email</label>
              <input
                type="email"
                required
                value={technicianForm.email}
                onChange={(e) =>
                  setTechnicianForm({
                    ...technicianForm,
                    email: e.target.value,
                  })
                }
              />

              <label>Specialization</label>
              <input
                required
                placeholder="e.g. Plumbing"
                value={technicianForm.specialization}
                onChange={(e) =>
                  setTechnicianForm({
                    ...technicianForm,
                    specialization: e.target.value,
                  })
                }
              />

              <label>Availability</label>
              <select
                value={technicianForm.availabilityStatus}
                onChange={(e) =>
                  setTechnicianForm({
                    ...technicianForm,
                    availabilityStatus: e.target.value,
                  })
                }
              >
                <option>AVAILABLE</option>
                <option>BUSY</option>
                <option>UNAVAILABLE</option>
              </select>

              <div className="modal-actions">
                <button
                  type="button"
                  className="secondary-button"
                  onClick={() => setShowTechnicianForm(false)}
                >
                  Cancel
                </button>

                <button className="primary-button" type="submit">
                  Add Technician
                </button>
              </div>

            </form>
          </div>
        </div>
      )}

      {selectedRequest && (
        <div className="modal-overlay">

          <div className="modal large-modal">

            <div className="modal-header">
              <div>
                <p className="eyebrow">MAINTENANCE REQUEST</p>
                <h2>Request #{selectedRequest.id}</h2>
              </div>

              <button onClick={() => setSelectedRequest(null)}>
                ×
              </button>
            </div>

            <div className="detail-grid">

              <div className="detail-section">
                <h3>Request Information</h3>

                <div className="detail-row">
                  <span>Description</span>
                  <strong>{selectedRequest.description}</strong>
                </div>

                <div className="detail-row">
                  <span>Property</span>
                  <strong>{selectedRequest.propertyId}</strong>
                </div>

                <div className="detail-row">
                  <span>Tenant</span>
                  <strong>{selectedRequest.tenantId}</strong>
                </div>

                <div className="detail-row">
                  <span>Category</span>
                  <strong>{selectedRequest.category}</strong>
                </div>

                <div className="detail-row">
                  <span>Priority</span>
                  <span className={priorityClass(selectedRequest.priority)}>
                    {selectedRequest.priority}
                  </span>
                </div>

                <div className="detail-row">
                  <span>Status</span>
                  <span className={statusClass(selectedRequest.status)}>
                    {selectedRequest.status}
                  </span>
                </div>
              </div>

              <div className="detail-section">
                <h3>Update Status</h3>

                <div className="status-buttons">
                  <button onClick={() => changeStatus("ASSIGNED")}>
                    Assigned
                  </button>

                  <button onClick={() => changeStatus("SCHEDULED")}>
                    Scheduled
                  </button>

                  <button onClick={() => changeStatus("IN_PROGRESS")}>
                    In Progress
                  </button>

                  <button onClick={() => changeStatus("RESOLVED")}>
                    Resolved
                  </button>
                </div>
              </div>

            </div>

            <div className="detail-section">
              <h3>Assign Technician</h3>

              <div className="technician-selection">
                {technicians
                  .filter(
                    (technician) =>
                      technician.availabilityStatus?.toUpperCase() ===
                      "AVAILABLE"
                  )
                  .map((technician) => (
                    <button
                      key={technician.id}
                      className="technician-option"
                      onClick={() =>
                        assignTechnician(technician.id)
                      }
                    >
                      <strong>{technician.name}</strong>
                      <span>{technician.specialization}</span>
                    </button>
                  ))}
              </div>
            </div>

            <div className="detail-section">
              <h3>Schedule Repair</h3>

              <form className="inline-form" onSubmit={scheduleRepair}>

                <select
                  required
                  value={scheduleForm.technicianId}
                  onChange={(e) =>
                    setScheduleForm({
                      ...scheduleForm,
                      technicianId: e.target.value,
                    })
                  }
                >
                  <option value="">Technician</option>

                  {technicians
                    .filter(
                      (t) =>
                        t.availabilityStatus?.toUpperCase() ===
                        "AVAILABLE"
                    )
                    .map((technician) => (
                      <option
                        key={technician.id}
                        value={technician.id}
                      >
                        {technician.name} -{" "}
                        {technician.specialization}
                      </option>
                    ))}
                </select>

                <input
                  required
                  type="date"
                  value={scheduleForm.scheduledDate}
                  onChange={(e) =>
                    setScheduleForm({
                      ...scheduleForm,
                      scheduledDate: e.target.value,
                    })
                  }
                />

                <input
                  required
                  type="time"
                  value={scheduleForm.startTime}
                  onChange={(e) =>
                    setScheduleForm({
                      ...scheduleForm,
                      startTime: e.target.value,
                    })
                  }
                />

                <input
                  required
                  type="time"
                  value={scheduleForm.endTime}
                  onChange={(e) =>
                    setScheduleForm({
                      ...scheduleForm,
                      endTime: e.target.value,
                    })
                  }
                />

                <button className="primary-button" type="submit">
                  Schedule
                </button>

              </form>
            </div>

            <div className="detail-section">
              <h3>Add Expense</h3>

              <form className="inline-form" onSubmit={addExpense}>

                <input
                  required
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="Amount"
                  value={expenseForm.amount}
                  onChange={(e) =>
                    setExpenseForm({
                      ...expenseForm,
                      amount: e.target.value,
                    })
                  }
                />

                <input
                  required
                  placeholder="Description"
                  value={expenseForm.description}
                  onChange={(e) =>
                    setExpenseForm({
                      ...expenseForm,
                      description: e.target.value,
                    })
                  }
                />

                <button className="primary-button" type="submit">
                  Add Expense
                </button>

              </form>
            </div>

            <div className="detail-section ai-section">
              <div>
                <span className="ai-label">AI ASSISTANT</span>
                <h3>Maintenance AI Recommendation</h3>
                <p>
                  AI analysis will appear here after the Gemini
                  integration is completed.
                </p>
              </div>

              <div className="ai-placeholder">
                <strong>AI Analysis Pending</strong>
                <span>
                  Category, priority, technician specialization and
                  recommended schedule will be displayed here.
                </span>
              </div>
            </div>

            <div className="detail-section">
              <h3>Status History</h3>

              {history.length === 0 ? (
                <p className="muted">
                  No status history available.
                </p>
              ) : (
                <div className="timeline">
                  {history.map((item) => (
                    <div className="timeline-item" key={item.id}>
                      <strong>
                        {item.oldStatus || "NEW"} → {item.newStatus}
                      </strong>

                      <span>
                        {item.comment || "Status updated"}
                      </span>

                      <small>
                        {item.changedAt
                          ? new Date(
                              item.changedAt
                            ).toLocaleString()
                          : ""}
                      </small>
                    </div>
                  ))}
                </div>
              )}
            </div>

          </div>
        </div>
      )}
    </div>
  );
}

export default MaintenanceDashboard;