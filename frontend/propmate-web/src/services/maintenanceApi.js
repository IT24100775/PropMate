const API_URL = "http://localhost:5235/api";
const AI_API_URL = "http://localhost:8000/api";

function authHeaders() {
  const savedUser = localStorage.getItem("propmate_user");
  if (!savedUser) return {};
  const user = JSON.parse(savedUser);
  return user.token ? { Authorization: `Bearer ${user.token}` } : {};
}

async function requestTo(baseUrl, url, options = {}) {
  const response = await fetch(`${baseUrl}${url}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...authHeaders(),
      ...(options.headers || {}),
    },
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed: ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

async function request(url, options = {}) {
  return requestTo(API_URL, url, options);
}

export const maintenanceApi = {
  getAll: (params = "") =>
    request(`/maintenance${params ? `?${params}` : ""}`),

  getById: (id) =>
    request(`/maintenance/${id}`),

  getForTenant: (tenantId) =>
    request(`/maintenance/tenant/${tenantId}`),

  getNotificationsForTenant: (tenantId) =>
    request(`/maintenance/tenant/${tenantId}/notifications`),

  create: (data) =>
    request("/maintenance", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  createForTenant: (data) =>
    request("/maintenance/tenant", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  update: (id, data) =>
    request(`/maintenance/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    }),

  assign: (id, data) =>
    request(`/maintenance/${id}/assign`, {
      method: "POST",
      body: JSON.stringify(data),
    }),

  schedule: (id, data) =>
    request(`/maintenance/${id}/schedule`, {
      method: "POST",
      body: JSON.stringify(data),
    }),

  updateStatus: (id, data) =>
    request(`/maintenance/${id}/status`, {
      method: "PATCH",
      body: JSON.stringify(data),
    }),

  addExpense: (id, data) =>
    request(`/maintenance/${id}/expenses`, {
      method: "POST",
      body: JSON.stringify(data),
    }),

  getExpenses: (id) =>
    request(`/maintenance/${id}/expenses`),

  getHistory: (id) =>
    request(`/maintenance/${id}/history`),

  getTechnicians: () =>
    request("/technician"),

  createTechnician: (data) =>
    request("/technician", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  updateTechnician: (id, data) =>
    request(`/technician/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    }),

  deleteTechnician: (id) =>
    request(`/technician/${id}`, {
      method: "DELETE",
    }),

  runAiWorkflow: (data) =>
    requestTo(AI_API_URL, "/maintenance-ai/run", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  approveAiWorkflow: (data) =>
    requestTo(AI_API_URL, "/maintenance-ai/approve", {
      method: "POST",
      body: JSON.stringify(data),
    }),
};