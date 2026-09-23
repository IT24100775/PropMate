const API_URL = "http://localhost:5235/api";

// Get available viewing slots for a property
export async function getViewingSlots(propertyId) {
    const response = await fetch(
        `${API_URL}/properties/${propertyId}/viewing-slots`
    );

    if (!response.ok) {
        throw new Error("Failed to load viewing slots.");
    }

    return await response.json();
}

// Create a new viewing slot
export async function createViewingSlot(
    propertyId,
    startTime,
    endTime
) {
    const response = await fetch(`${API_URL}/viewing-slots`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            propertyId,
            startTime,
            endTime,
        }),
    });

    if (!response.ok) {
        throw new Error("Failed to create viewing slot.");
    }

    return await response.json();
}

// Book a viewing slot
export async function bookViewing(viewingSlotId, userId) {
    const response = await fetch(`${API_URL}/viewing-bookings`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            viewingSlotId,
            userId,
        }),
    });

    if (!response.ok) {
        if (response.status === 409) {
            throw new Error("This viewing slot is already booked.");
        }

        throw new Error("Failed to book viewing.");
    }

    return await response.json();
}

// Cancel a viewing booking
export async function cancelViewing(bookingId) {
    const response = await fetch(
        `${API_URL}/viewing-bookings/${bookingId}`,
        {
            method: "DELETE",
        }
    );

    if (!response.ok) {
        throw new Error("Failed to cancel viewing.");
    }
}