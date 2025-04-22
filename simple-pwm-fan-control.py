# Script for controlling a PWM fan (e.g., 40mm 5V) on a Raspberry Pi.
# Sets the fan speed proportionally to the CPU temperature.
# Suitable for good quality fans.

import RPi.GPIO as IO          # Import the library to control GPIO pins
import time                    # Import the library to use delays
import subprocess              # Import the library to execute system commands (not actively used in the final code for temperature)

IO.setwarnings(False)          # Disable GPIO warning messages
IO.setmode (IO.BCM)            # Use BCM numbering for GPIO pins (GPIO14 corresponds to physical pin 8)
IO.setup(14,IO.OUT)            # Initialize GPIO14 as the fan output pin
fan = IO.PWM(14,100)           # Set GPIO14 as PWM output with 100Hz frequency (adjust to the fan's specified PWM frequency)
fan.start(0)                   # Start the PWM signal with a 0% duty cycle (fan off)

minTemp = 25                   # Minimum temperature (Celsius) to start the fan
maxTemp = 75                   # Maximum temperature (Celsius) to reach maximum speed
minSpeed = 0                   # Minimum fan speed (duty cycle percentage)
maxSpeed = 100                 # Maximum fan speed (duty cycle percentage)
MAX_FAN_SPEED = 100            # Maximum speed (percentage) to set when exiting the script

# Function to read the CPU temperature and return it as a float in degrees Celsius
def get_temp():
    try:
        # Read the temperature from the thermal sensor system file
        with open('/sys/class/thermal/thermal_zone0/temp', 'r') as f:
            temp_str = f.read()
        # Convert the read string (in millidegrees Celsius) to a float (degrees Celsius)
        return float(temp_str) / 1000.0
    except (FileNotFoundError, ValueError):
        # Handle errors if the file doesn't exist or the value is invalid
        raise RuntimeError('Could not read temperature from thermal zone')

# Function to map a value from an input range to an output range
def renormalize(input_value, input_range, output_range):
    input_span = input_range[1] - input_range[0]
    output_span = output_range[1] - output_range[0]
    # Scale the input value from the input range to the output range
    scaled_value = (output_span * (input_value - input_range[0]) / input_span) + output_range[0]
    return scaled_value

# Function to clear the console screen
def clear_console():
    print("\033[H\033[J", end="")  # ANSI escape code to clear screen and move cursor to home position

# Function to display the application interface
def display_interface(temperature, fan_speed):
    clear_console()
    
    # Application title
    title = "PWM FAN Control"
    border = "=" * len(title)
    print(f"\n{border}\n{title}\n{border}\n")
    
    # Format and display the values
    print(f"CPU Temperature: {temperature:5.1f}°C    |    Fan Speed: {fan_speed:3d}%\n")

try:
    while 1:                                    # Execute the loop indefinitely
        # Get the current CPU temperature
        raw_temp = get_temp()
        # Clamp the read temperature within the [minTemp, maxTemp] range
        clamped_temp = max(minTemp, min(raw_temp, maxTemp))
        # Calculate the fan speed by mapping the clamped temperature to the [minSpeed, maxSpeed] range
        fan_speed = int(renormalize(clamped_temp, [minTemp, maxTemp], [minSpeed, maxSpeed]))

        # Set the PWM duty cycle to control the fan speed
        fan.ChangeDutyCycle(fan_speed)

        # Display the interface with current values
        display_interface(raw_temp, fan_speed)

        # Wait 5 seconds before the next reading/update
        time.sleep(5)

except KeyboardInterrupt: # Catch keyboard interrupt (CTRL+C)
    print("\nExiting script. Setting fan to maximum speed.")
finally:                  # Block always executed before exiting (even in case of unhandled errors)
    fan.ChangeDutyCycle(MAX_FAN_SPEED) # Set fan speed to 100%
    IO.cleanup()          # Release GPIO resources used by the script
