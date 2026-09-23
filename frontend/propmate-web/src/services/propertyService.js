const API_URL = "http://localhost:5235/api/properties";

// Get all properties with optional search/filter parameters
export async function getProperties(filters = {}) {
    const params = new URLSearchParams();

    if (filters.search) params.append("search", filters.search);
    if (filters.minPrice) params.append("minPrice", filters.minPrice);
    if (filters.maxPrice) params.append("maxPrice", filters.maxPrice);
    if (filters.bedrooms) params.append("bedrooms", filters.bedrooms);
    if (filters.bathrooms) params.append("bathrooms", filters.bathrooms);
    if (filters.location) params.append("location", filters.location);
    if (filters.isAvailable !== undefined && filters.isAvailable !== "") {
        params.append("isAvailable", filters.isAvailable);
    }

    const queryString = params.toString();

    const response = await fetch(
        queryString ? `${API_URL}?${queryString}` : API_URL
    );

    if (!response.ok) {
        throw new Error("Failed to load properties.");
    }

    return await response.json();
}

// Get one property by ID
export async function getPropertyById(id) {
    const response = await fetch(`${API_URL}/${id}`);

    if (!response.ok) {
        throw new Error("Property not found.");
    }

    return await response.json();
}

// Get property location
export async function getPropertyLocation(id) {
    const response = await fetch(`${API_URL}/${id}/location`);

    if (!response.ok) {
        throw new Error("Property location not found.");
    }

    return await response.json();
}