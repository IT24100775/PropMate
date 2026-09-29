const API_URL = "http://localhost:5235/api/favourites";

function getAuthToken() {
    const savedUser = localStorage.getItem("propmate_user");

    if (!savedUser) {
        return null;
    }

    try {
        const user = JSON.parse(savedUser);
        return user.token || null;
    } catch {
        return null;
    }
}

// Add a property to favourites
export async function addFavourite(propertyId) {
    const token = getAuthToken();

    if (!token) {
        throw new Error("Please log in to add favourites.");
    }

    const response = await fetch(`${API_URL}/${propertyId}`, {
        method: "POST",
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });

    if (!response.ok) {
        if (response.status === 401) {
            throw new Error("Please log in to add favourites.");
        }

        if (response.status === 404) {
            throw new Error("Published property not found.");
        }

        if (response.status === 409) {
            throw new Error("Property is already in favourites.");
        }

        throw new Error("Failed to add favourite.");
    }

    return true;
}

// Remove a property from favourites
export async function removeFavourite(propertyId) {
    const token = getAuthToken();

    if (!token) {
        throw new Error("Please log in to remove favourites.");
    }

    const response = await fetch(`${API_URL}/${propertyId}`, {
        method: "DELETE",
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });

    if (!response.ok) {
        if (response.status === 401) {
            throw new Error("Please log in to remove favourites.");
        }

        if (response.status === 404) {
            throw new Error("Favourite not found.");
        }

        throw new Error("Failed to remove favourite.");
    }

    return true;
}