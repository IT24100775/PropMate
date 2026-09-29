import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getPropertyLocation } from "../../services/propertyService";

function PropertyLocation() {
    const { id } = useParams();
    const navigate = useNavigate();

    const [location, setLocation] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadLocation = async () => {
            try {
                setLoading(true);
                setError("");

                const data = await getPropertyLocation(id);

                setLocation(data);
            } catch (err) {
                setError(err.message || "Failed to load property location.");
            } finally {
                setLoading(false);
            }
        };

        loadLocation();
    }, [id]);

    if (loading) {
        return <p>Loading property location...</p>;
    }

    if (error) {
        return (
            <main className="property-location-page">
                <p role="alert">{error}</p>

                <button
                    type="button"
                    onClick={() => navigate(`/discover/${id}`)}
                >
                    Back to Property
                </button>
            </main>
        );
    }

    return (
        <main className="property-location-page">
            <button
                type="button"
                onClick={() => navigate(`/discover/${id}`)}
            >
                Back to Property
            </button>

            <section className="location-card">
                <h1>Property Location</h1>

                <p>
                    Location: {location.location}
                </p>

                <p>
                    Latitude: {location.latitude}
                </p>

                <p>
                    Longitude: {location.longitude}
                </p>

                <div className="map-placeholder">
                    <p>
                        Map integration can be added here using the
                        latitude and longitude.
                    </p>
                </div>
            </section>
        </main>
    );
}

export default PropertyLocation;