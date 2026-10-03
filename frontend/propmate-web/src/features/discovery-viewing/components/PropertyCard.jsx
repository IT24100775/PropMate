import { useNavigate } from "react-router-dom";

function PropertyCard({ property, onFavourite }) {
    const navigate = useNavigate();

    const handleViewDetails = () => {
        navigate(`/discover/${property.id}`);
    };

    return (
        <article className="property-card">
            <div className="property-card-content">
                <div className="property-card-header">
                    <h3>{property.title}</h3>

                    <span
                        className={
                            property.isAvailable
                                ? "property-status available"
                                : "property-status unavailable"
                        }
                    >
                        {property.isAvailable ? "Available" : "Unavailable"}
                    </span>
                </div>

                <p className="property-location">
                    {property.location}
                </p>

                <p className="property-description">
                    {property.description}
                </p>

                <div className="property-details">
                    <span> {property.bedrooms} Bedrooms</span>
                    <span> {property.bathrooms} Bathrooms</span>
                </div>

                <p className="property-price">
                    Rs. {Number(property.price).toLocaleString()}
                </p>

                <div className="property-card-actions">
                    <button
                        type="button"
                        onClick={handleViewDetails}
                    >
                        View Details
                    </button>

                    {onFavourite && (
                        <button
                            type="button"
                            onClick={() => onFavourite(property)}
                        >
                            Favourite
                        </button>
                    )}
                </div>
            </div>
        </article>
    );
}

export default PropertyCard;