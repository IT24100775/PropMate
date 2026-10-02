const API_URL = "http://localhost:5235/api";

function getAuthHeaders() {
    const savedUser = localStorage.getItem("propmate_user");

    if (!savedUser) {
        throw new Error("You must be logged in.");
    }

    const user = JSON.parse(savedUser);

    return {
        "Content-Type": "application/json",
        Authorization: `Bearer ${user.token}`,
    };
}

async function handleResponse(response) {
    if (response.status === 204) {
        return null;
    }

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
        if (response.status === 401) {
            throw new Error(
                "Your session has expired. Please log in again."
            );
        }

        if (response.status === 403) {
            throw new Error(
                "You do not have permission to perform this action."
            );
        }

        throw new Error(
            data?.message ||
            (typeof data === "string" ? data : null) ||
            `Request failed with status ${response.status}.`
        );
    }

    return data;
}

// ---------------------------------------------------------
// Buyer/Renter - available viewing slots
// ---------------------------------------------------------

export async function getViewingSlots(propertyId) {
    const response = await fetch(
        `${API_URL}/properties/${propertyId}/viewing-slots`
    );

    return handleResponse(response);
}

// ---------------------------------------------------------
// Owner/Agent - manage all slots for a property
// ---------------------------------------------------------

export async function getManagedViewingSlots(propertyId) {
    const response = await fetch(
        `${API_URL}/viewing-slots/property/${propertyId}/manage`,
        {
            method: "GET",
            headers: getAuthHeaders(),
        }
    );

    return handleResponse(response);
}

// ---------------------------------------------------------
// Owner/Agent - create viewing slot
// ---------------------------------------------------------

export async function createViewingSlot(
    propertyId,
    startTime,
    endTime
) {
    const response = await fetch(`${API_URL}/viewing-slots`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify({
            propertyId,
            startTime,
            endTime,
        }),
    });

    return handleResponse(response);
}

// ---------------------------------------------------------
// Owner/Agent - update viewing slot
// ---------------------------------------------------------

export async function updateViewingSlot(
    slotId,
    propertyId,
    startTime,
    endTime
) {
    const response = await fetch(
        `${API_URL}/viewing-slots/${slotId}`,
        {
            method: "PUT",
            headers: getAuthHeaders(),
            body: JSON.stringify({
                propertyId,
                startTime,
                endTime,
            }),
        }
    );

    return handleResponse(response);
}

// ---------------------------------------------------------
// Owner/Agent - delete viewing slot
// ---------------------------------------------------------

export async function deleteViewingSlot(slotId) {
    const response = await fetch(
        `${API_URL}/viewing-slots/${slotId}`,
        {
            method: "DELETE",
            headers: getAuthHeaders(),
        }
    );

    return handleResponse(response);
}

// ---------------------------------------------------------
// Buyer/Renter - book viewing
// ---------------------------------------------------------

export async function bookViewing(viewingSlotId) {
    const response = await fetch(`${API_URL}/viewing-bookings`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify({
            viewingSlotId,
        }),
    });

    return handleResponse(response);
}

// ---------------------------------------------------------
// Buyer/Renter - get own bookings
// ---------------------------------------------------------

export async function getMyViewingBookings() {
    const response = await fetch(
        `${API_URL}/viewing-bookings/my`,
        {
            method: "GET",
            headers: getAuthHeaders(),
        }
    );

    return handleResponse(response);
}

// ---------------------------------------------------------
// Buyer/Renter - cancel booking
// ---------------------------------------------------------

export async function cancelViewing(bookingId) {
    const response = await fetch(
        `${API_URL}/viewing-bookings/${bookingId}/cancel`,
        {
            method: "POST",
            headers: getAuthHeaders(),
        }
    );

    return handleResponse(response);
}