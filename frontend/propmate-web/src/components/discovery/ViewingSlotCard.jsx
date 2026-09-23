function ViewingSlotCard({ slot, onBook }) {
    const startTime = new Date(slot.startTime);
    const endTime = new Date(slot.endTime);

    const formattedDate = startTime.toLocaleDateString();
    const formattedStartTime = startTime.toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
    });
    const formattedEndTime = endTime.toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
    });

    return (
        <article className="viewing-slot-card">
            <div className="viewing-slot-info">
                <h3>Viewing Slot</h3>

                <p>
                    Date: {formattedDate}
                </p>

                <p>
                    Time: {formattedStartTime} - {formattedEndTime}
                </p>
            </div>

            <button
                type="button"
                onClick={() => onBook(slot)}
            >
                Book Viewing
            </button>
        </article>
    );
}

export default ViewingSlotCard;