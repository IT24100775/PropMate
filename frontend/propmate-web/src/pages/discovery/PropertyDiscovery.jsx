import { useEffect, useState } from "react";
import PropertyCard from "../../components/discovery/PropertyCard";
import PropertyFilters from "../../components/discovery/PropertyFilters";
import {
    getProperties,
} from "../../services/propertyService";
import {
    addFavourite,
} from "../../services/favouriteService";
import { useAuth } from "../../context/authContext";

const initialFilters = {
    search: "",
    minPrice: "",
    maxPrice: "",
    bedrooms: "",
    bathrooms: "",
    location: "",
    isAvailable: "",
};

function PropertyDiscovery() {
    const { user } = useAuth();

    const [properties, setProperties] = useState([]);
    const [filters, setFilters] = useState(initialFilters);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const loadProperties = async (currentFilters = filters) => {
        try {
            setLoading(true);
            setError("");

            const data = await getProperties(currentFilters);

            setProperties(data);
        } catch (err) {
            setError(err.message || "Failed to load properties.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadProperties(initialFilters);
    }, []);

    const handleSearch = () => {
        loadProperties(filters);
    };

    const handleReset = () => {
        setFilters(initialFilters);
        loadProperties(initialFilters);
    };

    const handleFavourite = async (property) => {
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

    return (
        <main className="property-discovery-page">
            <header className="discovery-header">
                <h1>Discover Properties</h1>
                <p>
                    Search and find properties that match your requirements.
                </p>
            </header>

            <PropertyFilters
                filters={filters}
                onChange={setFilters}
                onSearch={handleSearch}
                onReset={handleReset}
            />

            {loading && (
                <p>Loading properties...</p>
            )}

            {error && (
                <p role="alert">
                    {error}
                </p>
            )}

            {!loading && !error && properties.length === 0 && (
                <p>
                    No properties found. Try different search filters.
                </p>
            )}

            {!loading && properties.length > 0 && (
                <section className="property-grid">
                    {properties.map((property) => (
                        <PropertyCard
                            key={property.id}
                            property={property}
                            onFavourite={handleFavourite}
                        />
                    ))}
                </section>
            )}
        </main>
    );
}

export default PropertyDiscovery;