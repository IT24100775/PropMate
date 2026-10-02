function PropertyFilters({ filters, onChange, onSearch, onReset }) {
    const handleInputChange = (event) => {
        const { name, value } = event.target;

        onChange({
            ...filters,
            [name]: value,
        });
    };

    const handleSubmit = (event) => {
        event.preventDefault();
        onSearch();
    };

    return (
        <form className="property-filters" onSubmit={handleSubmit}>
            <div className="filter-group">
                <label htmlFor="search">Search</label>
                <input
                    id="search"
                    name="search"
                    type="text"
                    placeholder="Search properties..."
                    value={filters.search}
                    onChange={handleInputChange}
                />
            </div>

            <div className="filter-group">
                <label htmlFor="location">Location</label>
                <input
                    id="location"
                    name="location"
                    type="text"
                    placeholder="e.g. Colombo"
                    value={filters.location}
                    onChange={handleInputChange}
                />
            </div>

            <div className="filter-group">
                <label htmlFor="minPrice">Minimum Price</label>
                <input
                    id="minPrice"
                    name="minPrice"
                    type="number"
                    min="0"
                    placeholder="Minimum"
                    value={filters.minPrice}
                    onChange={handleInputChange}
                />
            </div>

            <div className="filter-group">
                <label htmlFor="maxPrice">Maximum Price</label>
                <input
                    id="maxPrice"
                    name="maxPrice"
                    type="number"
                    min="0"
                    placeholder="Maximum"
                    value={filters.maxPrice}
                    onChange={handleInputChange}
                />
            </div>

            <div className="filter-group">
                <label htmlFor="bedrooms">Minimum Bedrooms</label>
                <input
                    id="bedrooms"
                    name="bedrooms"
                    type="number"
                    min="0"
                    placeholder="Bedrooms"
                    value={filters.bedrooms}
                    onChange={handleInputChange}
                />
            </div>

            <div className="filter-group">
                <label htmlFor="bathrooms">Minimum Bathrooms</label>
                <input
                    id="bathrooms"
                    name="bathrooms"
                    type="number"
                    min="0"
                    placeholder="Bathrooms"
                    value={filters.bathrooms}
                    onChange={handleInputChange}
                />
            </div>

            <div className="filter-group">
                <label htmlFor="isAvailable">Availability</label>
                <select
                    id="isAvailable"
                    name="isAvailable"
                    value={filters.isAvailable}
                    onChange={handleInputChange}
                >
                    <option value="">All</option>
                    <option value="true">Available</option>
                    <option value="false">Unavailable</option>
                </select>
            </div>

            <div className="filter-actions">
                <button type="submit">
                    Search
                </button>

                <button
                    type="button"
                    onClick={onReset}
                >
                    Reset
                </button>
            </div>
        </form>
    );
}

export default PropertyFilters;