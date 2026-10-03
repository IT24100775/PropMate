const API =
    import.meta.env.VITE_API_URL || "http://localhost:5235/api";

function headers() {
    const saved = localStorage.getItem("propmate_user");

    if (!saved) {
        throw new Error("You must be logged in.");
    }

    const user = JSON.parse(saved);

    return {
        "Content-Type": "application/json",
        Accept: "application/json",
        Authorization: `Bearer ${user.token}`,
    };
}

// Normal API request.
// Any non-success response is treated as an error.
async function request(path, options = {}) {
    const response = await fetch(`${API}${path}`, {
        ...options,
        headers: {
            ...headers(),
            ...(options.headers || {}),
        },
    });

    const text = await response.text();
    let data = null;

    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }

    if (!response.ok) {
        throw new Error(
            data?.message ||
            data?.title ||
            (typeof data === "string"
                ? data
                : `Request failed (${response.status}).`)
        );
    }

    return data;
}

// Used for resources that may legitimately not exist yet.
// For example, a transaction may not have an agreement until
// the negotiation reaches the appropriate stage.
async function optionalRequest(path, options = {}) {
    const response = await fetch(`${API}${path}`, {
        ...options,
        headers: {
            ...headers(),
            ...(options.headers || {}),
        },
    });

    // No agreement exists yet — this is a valid state.
    if (response.status === 404) {
        return null;
    }

    const text = await response.text();
    let data = null;

    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }

    if (!response.ok) {
        throw new Error(
            data?.message ||
            data?.title ||
            (typeof data === "string"
                ? data
                : `Request failed (${response.status}).`)
        );
    }

    return data;
}

export const c3Api = {
    // =========================
    // RENTAL APPLICATIONS
    // =========================

    rentalMine: () =>
        request("/rental-applications/mine"),

    rentalOwner: () =>
        request("/rental-applications/owner"),

    rentalGet: (id) =>
        request(`/rental-applications/${id}`),

    rentalAccept: (id) =>
        request(`/rental-applications/${id}/accept`, {
            method: "POST",
        }),

    rentalReject: (id) =>
        request(`/rental-applications/${id}/reject`, {
            method: "POST",
        }),

    // =========================
    // PURCHASE OFFERS
    // =========================

    purchaseMine: () =>
        request("/purchase-offers/mine"),

    purchaseOwner: () =>
        request("/purchase-offers/owner"),

    purchaseGet: (id) =>
        request(`/purchase-offers/${id}`),

    purchaseAccept: (id) =>
        request(`/purchase-offers/${id}/accept`, {
            method: "POST",
        }),

    purchaseReject: (id) =>
        request(`/purchase-offers/${id}/reject`, {
            method: "POST",
        }),

    // =========================
    // RENTAL NEGOTIATION
    // =========================

    rentalOffers: (id) =>
        request(
            `/rental-applications/${id}/negotiation/offers`
        ),

    rentalMessages: (id) =>
        request(
            `/rental-applications/${id}/negotiation/messages`
        ),

    rentalCounter: (id, body) =>
        request(
            `/rental-applications/${id}/negotiation/counter`,
            {
                method: "POST",
                body: JSON.stringify(body),
            }
        ),

    rentalAcceptCounter: (id, offerId) =>
        request(
            `/rental-applications/${id}/negotiation/offers/${offerId}/accept`,
            {
                method: "POST",
            }
        ),

    rentalMessage: (id, body) =>
        request(
            `/rental-applications/${id}/negotiation/messages`,
            {
                method: "POST",
                body: JSON.stringify(body),
            }
        ),

    // 404 is allowed here because an agreement may not exist yet.
    rentalAgreement: (id) =>
        optionalRequest(
            `/rental-applications/${id}/agreement`
        ),

    rentalConfirm: (id) =>
        request(
            `/rental-applications/${id}/agreement/confirm`,
            {
                method: "POST",
            }
        ),

    // =========================
    // PURCHASE NEGOTIATION
    // =========================

    purchaseOffers: (id) =>
        request(
            `/purchase-offers/${id}/negotiation/offers`
        ),

    purchaseMessages: (id) =>
        request(
            `/purchase-offers/${id}/negotiation/messages`
        ),

    purchaseCounter: (id, body) =>
        request(
            `/purchase-offers/${id}/negotiation/counter`,
            {
                method: "POST",
                body: JSON.stringify(body),
            }
        ),

    purchaseAcceptCounter: (id, offerId) =>
        request(
            `/purchase-offers/${id}/negotiation/offers/${offerId}/accept`,
            {
                method: "POST",
            }
        ),

    purchaseMessage: (id, body) =>
        request(
            `/purchase-offers/${id}/negotiation/messages`,
            {
                method: "POST",
                body: JSON.stringify(body),
            }
        ),

    // 404 is allowed here because an agreement may not exist yet.
    purchaseAgreement: (id) =>
        optionalRequest(
            `/purchase-offers/${id}/agreement`
        ),

    purchaseConfirm: (id) =>
        request(
            `/purchase-offers/${id}/agreement/confirm`,
            {
                method: "POST",
            }
        ),

    // =========================
    // AGENTIC AI
    // =========================

    aiStart: (body) =>
        request("/ai-workflows", {
            method: "POST",
            body: JSON.stringify(body),
        }),

    aiGet: (id) =>
        request(`/ai-workflows/${id}`),

    aiApprove: (id, approvalId, body) =>
        request(
            `/ai-workflows/${id}/approvals/${approvalId}/decision`,
            {
                method: "POST",
                body: JSON.stringify(body),
            }
        ),
};