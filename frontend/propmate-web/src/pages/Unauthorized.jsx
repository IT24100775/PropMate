import { Link } from "react-router-dom";

function Unauthorized() {
  return (
    <div>
      <h1>Access Denied</h1>
      <p>
        You do not have permission to access this page.
      </p>

      <Link to="/">Return home</Link>
    </div>
  );
}

export default Unauthorized;