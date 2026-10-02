import { useEffect, useMemo, useState } from "react";
import OwnerSidebar from "../../components/owner/OwnerSidebar";
import { getOwnerListings } from "../../services/propertyListingService";
import {
    createViewingSlot,
    deleteViewingSlot,
    getManagedViewingSlots,
    updateViewingSlot,
} from "../../services/viewingService";
import "./ViewingManagement.css";

function formatDateTime(value) {
    if (!value) return "—";

    return new Date(value).toLocaleString("en-LK", {
        dateStyle: "medium",
        timeStyle: "short",
    });
}

function toDateTimeLocal(value) {
    if (!value) return "";

    const date = new Date(value);
    const offset = date.getTimezoneOffset();

    return new Date(date.getTime() - offset * 60 * 1000)
        .toISOString()
        .slice(0, 16);
}

function ViewingManagement() {
    const [listings, setListings] = useState([]);
    const [selectedPropertyId, setSelectedPropertyId] = useState("");
    const [managementData, setManagementData] = useState(null);

    const [startTime, setStartTime] = useState("");
    const [endTime, setEndTime] = useState("");

    const [editingSlotId, setEditingSlotId] = useState(null);

    const [loadingProperties, setLoadingProperties] = useState(true);
    const [loadingSlots, setLoadingSlots] = useState(false);
    const [saving, setSaving] = useState(false);
    const [actionId, setActionId] = useState(null);

    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    const publishedListings = useMemo(
        () =>
            listings.filter(
                (listing) => listing.status === "Published"
            ),
        [listings]
    );

    useEffect(() => {
        let cancelled = false;

        async function loadProperties() {
            try {
                setError("");

                const data = await getOwnerListings();

                if (!cancelled) {
                    setListings(data);

                    const published = data.filter(
                        (listing) => listing.status === "Published"
                    );

                    if (published.length > 0) {
                        setSelectedPropertyId(String(published[0].id));
                    }
                }
            } catch (err) {
                if (!cancelled) {
                    setError(err.message);
                }
            } finally {
                if (!cancelled) {
                    setLoadingProperties(false);
                }
            }
        }

        loadProperties();

        return () => {
            cancelled = true;
        };
    }, []);

    useEffect(() => {
        if (!selectedPropertyId) {
            setManagementData(null);
            return;
        }

        let cancelled = false;

        async function loadSlots() {
            try {
                setLoadingSlots(true);
                setError("");
                setSuccess("");

                const data = await getManagedViewingSlots(
                    selectedPropertyId
                );

                if (!cancelled) {
                    setManagementData(data);
                }
            } catch (err) {
                if (!cancelled) {
                    setError(err.message);
                    setManagementData(null);
                }
            } finally {
                if (!cancelled) {
                    setLoadingSlots(false);
                }
            }
        }

        loadSlots();

        return () => {
            cancelled = true;
        };
    }, [selectedPropertyId]);

    const refreshSlots = async () => {
        if (!selectedPropertyId) return;

        const data = await getManagedViewingSlots(
            selectedPropertyId
        );

        setManagementData(data);
    };

    const resetForm = () => {
        setStartTime("");
        setEndTime("");
        setEditingSlotId(null);
    };

    const handlePropertyChange = (event) => {
        setSelectedPropertyId(event.target.value);
        resetForm();
        setError("");
        setSuccess("");
    };

    const handleSubmit = async (event) => {
        event.preventDefault();

        if (!selectedPropertyId) {
            setError("Please select a property.");
            return;
        }

        if (!startTime || !endTime) {
            setError("Please select both start and end times.");
            return;
        }

        const start = new Date(startTime);
        const end = new Date(endTime);

        if (end <= start) {
            setError("End time must be after start time.");
            return;
        }

        try {
            setSaving(true);
            setError("");
            setSuccess("");

            if (editingSlotId) {
                await updateViewingSlot(
                    editingSlotId,
                    Number(selectedPropertyId),
                    start.toISOString(),
                    end.toISOString()
                );

                setSuccess("Viewing slot updated successfully.");
            } else {
                await createViewingSlot(
                    Number(selectedPropertyId),
                    start.toISOString(),
                    end.toISOString()
                );

                setSuccess("Viewing slot created successfully.");
            }

            resetForm();
            await refreshSlots();
        } catch (err) {
            setError(err.message);
        } finally {
            setSaving(false);
        }
    };

    const handleEdit = (slot) => {
        setEditingSlotId(slot.id);
        setStartTime(toDateTimeLocal(slot.startTime));
        setEndTime(toDateTimeLocal(slot.endTime));
        setError("");
        setSuccess("");

        window.scrollTo({
            top: 0,
            behavior: "smooth",
        });
    };

    const handleDelete = async (slot) => {
        const confirmed = window.confirm(
            "Delete this viewing slot?"
        );

        if (!confirmed) return;

        try {
            setActionId(slot.id);
            setError("");
            setSuccess("");

            await deleteViewingSlot(slot.id);

            if (editingSlotId === slot.id) {
                resetForm();
            }

            setSuccess("Viewing slot deleted successfully.");
            await refreshSlots();
        } catch (err) {
            setError(err.message);
        } finally {
            setActionId(null);
        }
    };

    const slots = managementData?.slots ?? [];

    const availableCount = slots.filter(
        (slot) => slot.isAvailable
    ).length;

    const bookedCount = slots.filter(
        (slot) => !slot.isAvailable
    ).length;

    return (
        <div className="owner-layout">
            <OwnerSidebar />

            <main className="owner-main viewing-management-main">
                <header className="viewing-page-header">
                    <div>
                        <p className="page-eyebrow">VIEWING MANAGEMENT</p>

                        <h1>Property viewings</h1>

                        <p className="page-description">
                            Create and manage viewing availability for your
                            published properties and keep track of booked slots.
                        </p>
                    </div>
                </header>

                {error && (
                    <div className="dashboard-error">{error}</div>
                )}

                {success && (
                    <div className="viewing-success">{success}</div>
                )}

                {loadingProperties ? (
                    <div className="dashboard-empty">
                        <p>Loading your properties...</p>
                    </div>
                ) : publishedListings.length === 0 ? (
                    <div className="dashboard-empty">
                        <div className="empty-number">00</div>

                        <h3>No published properties yet.</h3>

                        <p>
                            Viewing slots can be created once one of your
                            properties has been approved and published.
                        </p>
                    </div>
                ) : (
                    <>
                        <section className="viewing-property-section">
                            <div className="viewing-section-heading">
                                <div>
                                    <p className="page-eyebrow">
                                        PROPERTY SELECTION
                                    </p>

                                    <h2>Select a property</h2>
                                </div>

                                <span className="viewing-property-count">
                                    {publishedListings.length} published
                                </span>
                            </div>

                            <div className="viewing-property-selector">
                                <label htmlFor="viewing-property">
                                    PUBLISHED PROPERTY
                                </label>

                                <select
                                    id="viewing-property"
                                    value={selectedPropertyId}
                                    onChange={handlePropertyChange}
                                >
                                    {publishedListings.map((listing) => (
                                        <option
                                            key={listing.id}
                                            value={listing.id}
                                        >
                                            {listing.title} — {listing.city}
                                        </option>
                                    ))}
                                </select>
                            </div>
                        </section>

                        <section className="viewing-stats">
                            <div>
                                <span>TOTAL SLOTS</span>
                                <strong>
                                    {loadingSlots ? "—" : slots.length}
                                </strong>
                            </div>

                            <div>
                                <span>AVAILABLE</span>
                                <strong>
                                    {loadingSlots ? "—" : availableCount}
                                </strong>
                            </div>

                            <div>
                                <span>BOOKED</span>
                                <strong>
                                    {loadingSlots ? "—" : bookedCount}
                                </strong>
                            </div>
                        </section>

                        <section className="viewing-form-section">
                            <div className="viewing-section-heading">
                                <div>
                                    <p className="page-eyebrow">
                                        {editingSlotId
                                            ? "EDIT AVAILABILITY"
                                            : "NEW AVAILABILITY"}
                                    </p>

                                    <h2>
                                        {editingSlotId
                                            ? "Update viewing slot"
                                            : "Create viewing slot"}
                                    </h2>
                                </div>

                                {editingSlotId && (
                                    <button
                                        type="button"
                                        className="cancel-edit-button"
                                        onClick={resetForm}
                                    >
                                        Cancel edit
                                    </button>
                                )}
                            </div>

                            <form
                                className="viewing-slot-form"
                                onSubmit={handleSubmit}
                            >
                                <div className="viewing-form-field">
                                    <label htmlFor="viewing-start">
                                        START DATE & TIME
                                    </label>

                                    <input
                                        id="viewing-start"
                                        type="datetime-local"
                                        value={startTime}
                                        onChange={(event) =>
                                            setStartTime(event.target.value)
                                        }
                                    />
                                </div>

                                <div className="viewing-form-field">
                                    <label htmlFor="viewing-end">
                                        END DATE & TIME
                                    </label>

                                    <input
                                        id="viewing-end"
                                        type="datetime-local"
                                        value={endTime}
                                        onChange={(event) =>
                                            setEndTime(event.target.value)
                                        }
                                    />
                                </div>

                                <button
                                    type="submit"
                                    className="viewing-submit-button"
                                    disabled={saving}
                                >
                                    {saving
                                        ? "Saving..."
                                        : editingSlotId
                                            ? "Update slot"
                                            : "Create slot"}

                                    {!saving && <span>→</span>}
                                </button>
                            </form>
                        </section>

                        <section className="viewing-slots-section">
                            <div className="viewing-section-heading">
                                <div>
                                    <p className="page-eyebrow">
                                        VIEWING SCHEDULE
                                    </p>

                                    <h2>
                                        {managementData?.propertyTitle ||
                                            "Viewing slots"}
                                    </h2>
                                </div>

                                <span className="viewing-property-count">
                                    {slots.length} slots
                                </span>
                            </div>

                            {loadingSlots ? (
                                <div className="dashboard-empty">
                                    <p>Loading viewing slots...</p>
                                </div>
                            ) : slots.length === 0 ? (
                                <div className="dashboard-empty">
                                    <div className="empty-number">00</div>

                                    <h3>No viewing slots created.</h3>

                                    <p>
                                        Add the first available date and time for
                                        prospective renters or buyers.
                                    </p>
                                </div>
                            ) : (
                                <div className="viewing-slot-list">
                                    {slots.map((slot, index) => (
                                        <article
                                            className="viewing-slot-card"
                                            key={slot.id}
                                        >
                                            <div className="viewing-slot-index">
                                                {String(index + 1).padStart(2, "0")}
                                            </div>

                                            <div className="viewing-slot-time">
                                                <span>VIEWING WINDOW</span>

                                                <strong>
                                                    {formatDateTime(slot.startTime)}
                                                </strong>

                                                <small>
                                                    to {formatDateTime(slot.endTime)}
                                                </small>
                                            </div>

                                            <div className="viewing-slot-status">
                                                <span
                                                    className={
                                                        slot.isAvailable
                                                            ? "viewing-status available"
                                                            : "viewing-status booked"
                                                    }
                                                >
                                                    {slot.isAvailable
                                                        ? "Available"
                                                        : "Booked"}
                                                </span>
                                            </div>

                                            <div className="viewing-booking-info">
                                                {slot.booking ? (
                                                    <>
                                                        <span>BOOKED BY</span>

                                                        <strong>
                                                            {slot.booking.userEmail ||
                                                                `User #${slot.booking.userId}`}
                                                        </strong>

                                                        <small>
                                                            Booked{" "}
                                                            {formatDateTime(
                                                                slot.booking.bookedAt
                                                            )}
                                                        </small>
                                                    </>
                                                ) : (
                                                    <>
                                                        <span>BOOKING</span>
                                                        <strong>No booking yet</strong>
                                                        <small>
                                                            Waiting for a buyer or renter
                                                        </small>
                                                    </>
                                                )}
                                            </div>

                                            <div className="viewing-slot-actions">
                                                <button
                                                    type="button"
                                                    className="viewing-edit-button"
                                                    onClick={() => handleEdit(slot)}
                                                    disabled={
                                                        actionId === slot.id ||
                                                        !slot.isAvailable
                                                    }
                                                >
                                                    Edit
                                                </button>

                                                <button
                                                    type="button"
                                                    className="viewing-delete-button"
                                                    onClick={() => handleDelete(slot)}
                                                    disabled={
                                                        actionId === slot.id ||
                                                        !slot.isAvailable
                                                    }
                                                >
                                                    {actionId === slot.id
                                                        ? "Deleting..."
                                                        : "Delete"}
                                                </button>
                                            </div>
                                        </article>
                                    ))}
                                </div>
                            )}
                        </section>
                    </>
                )}
            </main>
        </div>
    );
}

export default ViewingManagement;