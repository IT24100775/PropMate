import subprocess
import time
subprocess.Popen(["python", "-m", "uvicorn", "main:app", "--port", "8000"])
print("Started")
