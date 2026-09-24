import { useEffect, useState } from "react";
import { getProperties, createProperty, updateProperty, deleteProperty } from "../../services/propertyService";

function PropertyManagement() {
    const [properties, setProperties] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [message, setMessage] = useState("");
    const [editingProperty, setEditingProperty] = useState(null);

    const [form, setForm] = useState({
        title: "",
        description: "",
        price: "",
        bedrooms: "",
        bathrooms: "",
        location: "",
        latitude: "",
        longitude: "",
        isAvailable: true
    });

    const loadProperties = async () => {
        try {
            setLoading(true);
            setError("");

            const data = await getProperties();
            setProperties(data);
        } catch (err) {
            console.error(err);
            setError("Failed to load properties.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadProperties();
    }, []);

    const handleChange = (event) => {
        const { name, value, type, checked } = event.target;

        setForm((previous) => ({
            ...previous,
            [name]: type === "checkbox" ? checked : value
        }));
    };

    const handleDelete = async (id) => {
        if (!window.confirm("Are you sure you want to delete this property?")) {
            return;
        }

        try {
            setError("");
            setMessage("");

            await deleteProperty(id);

            setProperties((currentProperties) =>
                currentProperties.filter((property) => property.id !== id)
            );

            setMessage("Property deleted successfully.");
        } catch (err) {
            console.error(err);
            setError("Failed to delete property.");
        }
    };
    const handleEdit = (property) => {
        setEditingProperty({
            id: property.id,
            title: property.title,
            description: property.description,
            price: property.price,
            bedrooms: property.bedrooms,
            bathrooms: property.bathrooms,
            location: property.location,
            latitude: property.latitude,
            longitude: property.longitude,
            isAvailable: property.isAvailable
        });

        setMessage("");
        setError("");
    };

    const handleUpdate = async (event) => {
        event.preventDefault();

        try {
            setError("");
            setMessage("");

            await updateProperty(
                editingProperty.id,
                editingProperty
            );

            setMessage("Property updated successfully.");
            setEditingProperty(null);

            await loadProperties();
        } catch (err) {
            console.error(err);
            setError("Failed to update property.");
        }
    };

    const handleEditChange = (event) => {
        const { name, value, type, checked } = event.target;

        setEditingProperty((previous) => ({
            ...previous,
            [name]: type === "checkbox" ? checked : value
        }));
    };

    const handleSubmit = async (event) => {
        event.preventDefault();

        try {
            setError("");
            setMessage("");

            const propertyData = {
                title: form.title,
                description: form.description,
                price: Number(form.price),
                bedrooms: Number(form.bedrooms),
                bathrooms: Number(form.bathrooms),
                location: form.location,
                latitude: Number(form.latitude),
                longitude: Number(form.longitude),
                isAvailable: form.isAvailable
            };

            await createProperty(propertyData);

            setMessage("Property created successfully.");

            setForm({
                title: "",
                description: "",
                price: "",
                bedrooms: "",
                bathrooms: "",
                location: "",
                latitude: "",
                longitude: "",
                isAvailable: true
            });

            await loadProperties();
        } catch (err) {
            console.error(err);
            setError("Failed to create property.");
        }
    };

    if (loading) {
        return <div style={{ padding: "24px" }}>Loading properties...</div>;
    }

    return (
        <div style={{ padding: "24px", maxWidth: "1000px" }}>
            <h1>Property Management</h1>
            <p>Owner / Admin / Product Manager</p>

            <hr />

            <h2>Add New Property</h2>

            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: "12px" }}>
                    <label>Title</label><br />
                    <input
                        name="title"
                        value={form.title}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label>Description</label><br />
                    <textarea
                        name="description"
                        value={form.description}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label>Price</label><br />
                    <input
                        type="number"
                        name="price"
                        value={form.price}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label>Bedrooms</label><br />
                    <input
                        type="number"
                        name="bedrooms"
                        value={form.bedrooms}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label>Bathrooms</label><br />
                    <input
                        type="number"
                        name="bathrooms"
                        value={form.bathrooms}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label>Location</label><br />
                    <input
                        name="location"
                        value={form.location}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label>Latitude</label><br />
                    <input
                        type="number"
                        step="any"
                        name="latitude"
                        value={form.latitude}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label>Longitude</label><br />
                    <input
                        type="number"
                        step="any"
                        name="longitude"
                        value={form.longitude}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label>
                        <input
                            type="checkbox"
                            name="isAvailable"
                            checked={form.isAvailable}
                            onChange={handleChange}
                        />
                        {" "}Available
                    </label>
                </div>

                <button type="submit">
                    Add Property
                </button>
            </form>

            {message && (
                <p style={{ color: "green" }}>{message}</p>
            )}

            {error && (
                <p style={{ color: "red" }}>{error}</p>
            )}

            <hr />

            <h2>Existing Properties</h2>

            {editingProperty && (
                <div
                    style={{
                        border: "2px solid #333",
                        padding: "16px",
                        marginBottom: "20px",
                        borderRadius: "8px"
                    }}
                >
                    <h2>Edit Property</h2>

                    <form onSubmit={handleUpdate}>
                        <div style={{ marginBottom: "12px" }}>
                            <label>Title</label><br />
                            <input
                                name="title"
                                value={editingProperty.title}
                                onChange={handleEditChange}
                                required
                            />
                        </div>

                        <div style={{ marginBottom: "12px" }}>
                            <label>Description</label><br />
                            <textarea
                                name="description"
                                value={editingProperty.description}
                                onChange={handleEditChange}
                                required
                            />
                        </div>

                        <div style={{ marginBottom: "12px" }}>
                            <label>Price</label><br />
                            <input
                                type="number"
                                name="price"
                                value={editingProperty.price}
                                onChange={handleEditChange}
                                required
                            />
                        </div>

                        <div style={{ marginBottom: "12px" }}>
                            <label>Bedrooms</label><br />
                            <input
                                type="number"
                                name="bedrooms"
                                value={editingProperty.bedrooms}
                                onChange={handleEditChange}
                                required
                            />
                        </div>

                        <div style={{ marginBottom: "12px" }}>
                            <label>Bathrooms</label><br />
                            <input
                                type="number"
                                name="bathrooms"
                                value={editingProperty.bathrooms}
                                onChange={handleEditChange}
                                required
                            />
                        </div>

                        <div style={{ marginBottom: "12px" }}>
                            <label>Location</label><br />
                            <input
                                name="location"
                                value={editingProperty.location}
                                onChange={handleEditChange}
                                required
                            />
                        </div>

                        <div style={{ marginBottom: "12px" }}>
                            <label>Latitude</label><br />
                            <input
                                type="number"
                                step="any"
                                name="latitude"
                                value={editingProperty.latitude}
                                onChange={handleEditChange}
                                required
                            />
                        </div>

                        <div style={{ marginBottom: "12px" }}>
                            <label>Longitude</label><br />
                            <input
                                type="number"
                                step="any"
                                name="longitude"
                                value={editingProperty.longitude}
                                onChange={handleEditChange}
                                required
                            />
                        </div>

                        <div style={{ marginBottom: "12px" }}>
                            <label>
                                <input
                                    type="checkbox"
                                    name="isAvailable"
                                    checked={editingProperty.isAvailable}
                                    onChange={handleEditChange}
                                />
                                {" "}Available
                            </label>
                        </div>

                        <button type="submit">
                            Save Changes
                        </button>

                        {" "}

                        <button
                            type="button"
                            onClick={() => setEditingProperty(null)}
                        >
                            Cancel
                        </button>
                    </form>
                </div>
            )}

            {properties.length === 0 ? (
                <p>No properties found.</p>
            ) : (
                properties.map((property) => (
                    <div
                        key={property.id}
                        style={{
                            border: "1px solid #ddd",
                            padding: "16px",
                            marginBottom: "12px",
                            borderRadius: "8px"
                        }}
                    >
                        <h3>{property.title}</h3>

                        <p>{property.description}</p>

                        <p>
                            <strong>Location:</strong>{" "}
                            {property.location}
                        </p>

                        <p>
                            <strong>Price:</strong>{" "}
                            Rs. {property.price?.toLocaleString()}
                        </p>

                        <p>
                            <strong>Bedrooms:</strong>{" "}
                            {property.bedrooms}
                        </p>

                        <p>
                            <strong>Bathrooms:</strong>{" "}
                            {property.bathrooms}
                        </p>

                        <p>
                            <strong>Status:</strong>{" "}
                            {property.isAvailable
                                ? "Available"
                                : "Not Available"}
                        </p>

                        <button
                            type="button"
                            onClick={() => handleEdit(property)}
                        >
                            Edit
                        </button>

                        <button
                            type="button"
                            onClick={() => handleDelete(property.id)}
                            style={{ marginLeft: "8px" }}
                        >
                            Delete
                        </button>
                    </div>
                ))
            )}
        </div>
    );
}

export default PropertyManagement;






