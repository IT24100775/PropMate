const API = "http://localhost:5235/api";

function headers() {
  const savedUser = localStorage.getItem("propmate_user");
  if (!savedUser) throw new Error("You must be logged in.");

  const user = JSON.parse(savedUser);
  return {
    "Content-Type": "application/json",
    Accept: "application/json",
    Authorization: `Bearer ${user.token}`,
  };
}

async function request(path, options = {}) {
  const response = await fetch(`${API}${path}`, {
    ...options,
    headers: { ...headers(), ...(options.headers || {}) },
  });
  const text = await response.text();
  let data = null;
  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      data = text;
    }
  }

  if (!response.ok) {
    throw new Error(
      data?.message ||
        data?.title ||
        (typeof data === "string" ? data : `Request failed (${response.status}).`),
    );
  }

  return data;
}

export const c3Api = {
  rentalCreate: (body) =>
    request("/rental-applications", {
      method: "POST",
      body: JSON.stringify(body),
    }),
  rentalMine: () => request("/rental-applications/mine"),
  rentalOwner: () => request("/rental-applications/owner"),
  rentalGet: (id) => request(`/rental-applications/${id}`),
  rentalAccept: (id) => request(`/rental-applications/${id}/accept`, { method: "POST" }),
  rentalReject: (id) => request(`/rental-applications/${id}/reject`, { method: "POST" }),
  purchaseCreate: (body) =>
    request("/purchase-offers", {
      method: "POST",
      body: JSON.stringify(body),
    }),
  purchaseMine: () => request("/purchase-offers/mine"),
  purchaseOwner: () => request("/purchase-offers/owner"),
  purchaseGet: (id) => request(`/purchase-offers/${id}`),
  purchaseAccept: (id) => request(`/purchase-offers/${id}/accept`, { method: "POST" }),
  purchaseReject: (id) => request(`/purchase-offers/${id}/reject`, { method: "POST" }),
  rentalOffers: (id) => request(`/rental-applications/${id}/negotiation/offers`),
  rentalMessages: (id) => request(`/rental-applications/${id}/negotiation/messages`),
  rentalCounter: (id, body) =>
    request(`/rental-applications/${id}/negotiation/counter`, {
      method: "POST",
      body: JSON.stringify(body),
    }),
  rentalAcceptCounter: (id, offerId) =>
    request(`/rental-applications/${id}/negotiation/offers/${offerId}/accept`, { method: "POST" }),
  rentalMessage: (id, body) =>
    request(`/rental-applications/${id}/negotiation/messages`, {
      method: "POST",
      body: JSON.stringify(body),
    }),
  rentalAgreement: (id) => request(`/rental-applications/${id}/agreement`),
  rentalConfirm: (id) =>
    request(`/rental-applications/${id}/agreement/confirm`, { method: "POST" }),
  purchaseOffers: (id) => request(`/purchase-offers/${id}/negotiation/offers`),
  purchaseMessages: (id) => request(`/purchase-offers/${id}/negotiation/messages`),
  purchaseCounter: (id, body) =>
    request(`/purchase-offers/${id}/negotiation/counter`, {
      method: "POST",
      body: JSON.stringify(body),
    }),
  purchaseAcceptCounter: (id, offerId) =>
    request(`/purchase-offers/${id}/negotiation/offers/${offerId}/accept`, { method: "POST" }),
  purchaseMessage: (id, body) =>
    request(`/purchase-offers/${id}/negotiation/messages`, {
      method: "POST",
      body: JSON.stringify(body),
    }),
  purchaseAgreement: (id) => request(`/purchase-offers/${id}/agreement`),
  purchaseConfirm: (id) =>
    request(`/purchase-offers/${id}/agreement/confirm`, { method: "POST" }),
  aiStart: (body) => request("/ai-workflows", { method: "POST", body: JSON.stringify(body) }),
  aiGet: (id) => request(`/ai-workflows/${id}`),
  aiApprove: (id, approvalId, body) =>
    request(`/ai-workflows/${id}/approvals/${approvalId}/decision`, {
      method: "POST",
      body: JSON.stringify(body),
    }),
};
