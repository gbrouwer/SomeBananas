from pathlib import Path
import subprocess

# Full command
config_path = Path("C:/Core/SomeBananas/ml-agents/config/ppo/TrafficJam.yaml")
cmd = Path("C:/Core/SomeBananas/.venv/Scripts/mlagents-learn.exe")
arguments = "--run-id=TrafficJam --force"

full_command = [
    str(cmd),
    str(config_path),
] + arguments.split()

# Debug: print the command before running
print("Running command:", full_command)
print(f"Running command: {full_command}")
subprocess.run(full_command)
