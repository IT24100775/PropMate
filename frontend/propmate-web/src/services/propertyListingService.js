const API_URL = "http://localhost:5235/api/PropertyListings";

export async function getPublishedListings() {
  const response = await fetch(`${API_URL}?page=1&pageSize=100`, {
    headers: { Accept: "application/json" },
  });
  const data = await handleResponse(response);
  return data?.items || [];
}

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

export async function getAdminListings() {
  const response = await fetch(`${API_URL}/admin`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function getVerificationReview(id) {
  const response = await fetch(`${API_URL}/${id}/verification-review`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function approveListing(id, reason = "") {
  const response = await fetch(`${API_URL}/${id}/approve`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify({
      reason: reason || null,
    }),
  });

  return handleResponse(response);
}

export async function rejectListing(id, reason) {
  const response = await fetch(`${API_URL}/${id}/reject`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify({ reason }),
  });

  return handleResponse(response);
}

export async function requestListingRevision(id, reason) {
  const response = await fetch(
    `${API_URL}/${id}/request-revision`,
    {
      method: "POST",
      headers: getAuthHeaders(),
      body: JSON.stringify({ reason }),
    }
  );

  return handleResponse(response);
}

export async function publishListing(id) {
  const response = await fetch(`${API_URL}/${id}/publish`, {
    method: "POST",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function unpublishListing(id) {
  const response = await fetch(`${API_URL}/${id}/unpublish`, {
    method: "POST",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}