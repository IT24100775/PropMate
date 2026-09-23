import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getPropertyById } from "../../services/propertyService";
import { addFavourite } from "../../services/favouriteService";
import { useAuth } from "../../context/authContext";

function PropertyDetails() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user } = useAuth();

    const [property, setProperty] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadProperty = async () => {
            try {
                setLoading(true);
                setError("");

                const data = await getPropertyById(id);

                setProperty(data);
            } catch (err) {
                setError(err.message || "Failed to load property.");
            } finally {
                setLoading(false);
            }
        };

        loadProperty();
    }, [id]);

    const handleFavourite = async () => {
        if (!user) {
            setError("Please log in to add favourites.");
            return;
        }

        try {
            setError("");

            await addFavourite(property.id, user.userId);

            alert("Property added to favourites.");
        } catch (err) {
            setError(err.message || "Failed to add favourite.");
        }
    };

    if (loading) {
        return <p>Loading property details...</p>;
    }

    if (error && !property) {
        return (
            <main>
                <p role="alert">{error}</p>

                <button
                    type="button"
                    onClick={() => navigate("/discover")}
                >
                    Back to Properties
                </button>
            </main>
        );
    }

    return (
        <main className="property-details-page">
            <button
                type="button"
                onClick={() => navigate("/discover")}
            >
                Back to Properties
            </button>

            <section className="property-details">
                <h1>{property.title}</h1>

                <p>
                    Location: {property.location}
                </p>

                <p>
                    {property.description}
                </p>

                <div className="property-details-info">
                    <p>
                        Price: Rs.{" "}
                        {Number(property.price).toLocaleString()}
                    </p>

                    <p>
                        Bedrooms: {property.bedrooms}
                    </p>

                    <p>
                        Bathrooms: {property.bathrooms}
                    </p>

                    <p>
                        Status:{" "}
                        {property.isAvailable
                            ? "Available"
                            : "Unavailable"}
                    </p>
                </div>

                <div className="property-details-actions">
                    <button
                        type="button"
                        onClick={handleFavourite}
                    >
                        Add to Favourite
                    </button>

                    {property.isAvailable && (
                        <>
                            <button
                                type="button"
                                onClick={() =>
                                    navigate(
                                        `/discover/${property.id}/location`
                                    )
                                }
                            >
                                View Location
                            </button>

                            <button
                                type="button"
                                onClick={() =>
                                    navigate(
                                        `/discover/${property.id}/viewing-slots`
                                    )
                                }
                            >
                                View Viewing Slots
                            </button>
                        </>
                    )}
                </div>

                {error && (
                    <p role="alert">
                        {error}
                    </p>
                )}
            </section>
        </main>
    );
}

export default PropertyDetails;