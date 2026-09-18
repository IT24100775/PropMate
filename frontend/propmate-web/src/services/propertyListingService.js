const API_URL = "http://localhost:5235/api/PropertyListings";

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

  const data = await response.json();

  if (!response.ok) {
    throw new Error(data.message || "Something went wrong.");
  }

  return data;
}

export async function getOwnerListings() {
  const response = await fetch(`${API_URL}/owner`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function getOwnerListingById(id) {
  const listings = await getOwnerListings();

  const listing = listings.find(
    (item) => Number(item.id) === Number(id)
  );

  if (!listing) {
    throw new Error("Property listing not found.");
  }

  return listing;
}

export async function createListing(listingData) {
  const response = await fetch(API_URL, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(listingData),
  });

  return handleResponse(response);
}

export async function updateListing(id, listingData) {
  const response = await fetch(`${API_URL}/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(listingData),
  });

  return handleResponse(response);
}

export async function deleteListing(id) {
  const response = await fetch(`${API_URL}/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function submitListing(id) {
  const response = await fetch(`${API_URL}/${id}/submit`, {
    method: "POST",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}