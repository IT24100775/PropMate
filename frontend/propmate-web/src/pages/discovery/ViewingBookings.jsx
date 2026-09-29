import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import ViewingSlotCard from "../../components/discovery/ViewingSlotCard";
import { getViewingSlots, bookViewing } from "../../services/viewingService";
import { useAuth } from "../../context/authContext";

function ViewingBookings() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user } = useAuth();

    const [slots, setSlots] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const loadSlots = async () => {
        try {
            setLoading(true);
            setError("");

            const data = await getViewingSlots(id);

            setSlots(data);
        } catch (err) {
            setError(err.message || "Failed to load viewing slots.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadSlots();
    }, [id]);

    const handleBook = async (slot) => {
        if (!user) {
            setError("Please log in to book a viewing.");
            return;
        }

        try {
            setError("");

            await bookViewing(slot.id, user.userId);

            alert("Viewing booked successfully.");

            await loadSlots();
        } catch (err) {
            setError(err.message || "Failed to book viewing.");
        }
    };

    return (
        <main className="viewing-bookings-page">
            <button
                type="button"
                onClick={() => navigate(`/discover/${id}`)}
            >
                Back to Property
            </button>

            <header>
                <h1>Viewing Slots</h1>
                <p>
                    Select an available time slot to view this property.
                </p>
            </header>

            {loading && (
                <p>Loading viewing slots...</p>
            )}

            {error && (
                <p role="alert">
                    {error}
                </p>
            )}

            {!loading && !error && slots.length === 0 && (
                <p>
                    No viewing slots are currently available.
                </p>
            )}

            {!loading && slots.length > 0 && (
                <section className="viewing-slots-list">
                    {slots.map((slot) => (
                        <ViewingSlotCard
                            key={slot.id}
                            slot={slot}
                            onBook={handleBook}
                        />
                    ))}
                </section>
            )}
        </main>
    );
}

export default ViewingBookings;