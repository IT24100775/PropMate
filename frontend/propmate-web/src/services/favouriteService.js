const API_URL = "http://localhost:5235/api/favourites";

// Add a property to favourites
export async function addFavourite(propertyId, userId) {
    const response = await fetch(API_URL, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            propertyId,
            userId,
        }),
    });

    if (!response.ok) {
        if (response.status === 409) {
            throw new Error("Property is already in favourites.");
        }

        throw new Error("Failed to add favourite.");
    }

    return await response.json();
}

// Remove a favourite
export async function removeFavourite(favouriteId) {
    const response = await fetch(`${API_URL}/${favouriteId}`, {
        method: "DELETE",
    });

    if (!response.ok) {
        throw new Error("Failed to remove favourite.");
    }
}