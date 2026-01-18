#!/bin/bash
# Kill process running on port 5100

PORT=5100
PID=$(lsof -ti:$PORT)

if [[ -z "$PID" ]]; then
    echo "No process found running on port $PORT"
else
    echo "Killing process $PID running on port $PORT"
    kill -9 $PID
    echo "Process killed successfully"
fi
