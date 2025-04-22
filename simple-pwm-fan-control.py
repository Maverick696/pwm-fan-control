# Script for controlling a PWM fan (e.g., 40mm 5V) on a Raspberry Pi.
# Sets the fan speed proportionally to the CPU temperature.
# Suitable for good quality fans.

import RPi.GPIO as IO          # Import the library to control GPIO pins
import time                    # Import the library to use delays
import subprocess              # Import the library to execute system commands (not actively used in the final code for temperature)
import collections             # Import for storing historical data

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

# Constants for graph
GRAPH_WIDTH = 60              # Width of the graph in characters
GRAPH_HEIGHT = 10             # Height of the graph in characters
MAX_HISTORY = GRAPH_WIDTH     # Maximum number of data points to store

# Store historical data
temp_history = collections.deque(maxlen=MAX_HISTORY)
fan_history = collections.deque(maxlen=MAX_HISTORY)

# ANSI color codes
RED = "\033[31m"
BLUE = "\033[34m"
WHITE = "\033[37m"
RESET = "\033[0m"

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

# Function to draw the ASCII graph (CPU temperature only)
def draw_graph():
    # If we don't have enough data yet, return an empty string
    if not temp_history:
        return "Collecting data for graph...\n"
    
    # Create a 2D array to represent the graph (filled with spaces)
    graph = [[' ' for _ in range(GRAPH_WIDTH)] for _ in range(GRAPH_HEIGHT)]
    
    # Calculate the range for the temperature scale
    temp_min = min(minTemp, min(temp_history)) if temp_history else minTemp
    temp_max = max(maxTemp, max(temp_history)) if temp_history else maxTemp
    temp_range = temp_max - temp_min
    
    # Draw the temperature line (red)
    for i, temp in enumerate(temp_history):
        if i >= GRAPH_WIDTH:
            break
        # Calculate the y position (inverted because higher index is lower on screen)
        y_pos = GRAPH_HEIGHT - 1 - int((temp - temp_min) / temp_range * (GRAPH_HEIGHT - 1))
        y_pos = max(0, min(GRAPH_HEIGHT - 1, y_pos))  # Ensure it's within bounds
        graph[y_pos][i] = RED + "●" + RESET
    
    # Build the graph string
    result = []
    
    # Add the temperature scale on the y-axis
    for i in range(GRAPH_HEIGHT):
        # Calculate the temperature for this line
        temp_value = temp_max - (i * temp_range / (GRAPH_HEIGHT - 1))
        # Add the temperature label every few lines
        if i == 0 or i == GRAPH_HEIGHT - 1 or i == GRAPH_HEIGHT // 2:
            y_label = f"{temp_value:4.1f}°C |"
        else:
            y_label = "       |"
        
        # Add the row with data
        row = y_label + ''.join(graph[i])
        result.append(row)
    
    # Add the x-axis (time)
    result.append("       " + "+" + "-" * GRAPH_WIDTH)
    result.append("       " + "Time →")
    
    # Add the legend for CPU temperature only
    legend = f"  Legend: {RED}● CPU Temperature{RESET}"
    result.append(legend)
    
    return "\n".join(result)

# Function to draw a fan speed gauge
def draw_fan_gauge(fan_speed):
    # Gauge width and character for filling
    gauge_width = GRAPH_WIDTH
    fill_char = "█"
    empty_char = "░"
    
    # Calculate how many characters to fill based on fan speed percentage
    fill_width = int((fan_speed / 100) * gauge_width)
    
    # Create the gauge bar
    gauge_bar = BLUE + fill_char * fill_width + empty_char * (gauge_width - fill_width) + RESET
    
    # Create the gauge labels
    gauge_label = "Fan Speed: "
    
    # Build the gauge display
    result = []
    result.append(f"\n{gauge_label}{gauge_bar} {fan_speed}%")
    result.append(f"0%{' ' * (gauge_width - 5)}100%")
    
    return "\n".join(result)

# Function to display the application interface
def display_interface(temperature, fan_speed):
    clear_console()
    
    # Application title
    title = "PWM FAN Control"
    border = "=" * len(title)
    print(f"\n{border}\n{title}\n{border}\n")
    
    # Format and display the values
    print(f"CPU Temperature: {temperature:5.1f}°C    |    Fan Speed: {fan_speed:3d}%\n")
    
    # Add the temperature graph
    print(draw_graph())
    
    # Add the fan speed gauge below the graph
    print(draw_fan_gauge(fan_speed))

try:
    while 1:                                    # Execute the loop indefinitely
        # Get the current CPU temperature
        raw_temp = get_temp()
        # Clamp the read temperature within the [minTemp, maxTemp] range
        clamped_temp = max(minTemp, min(raw_temp, maxTemp))
        # Calculate the fan speed by mapping the clamped temperature to the [minSpeed, maxSpeed] range
        fan_speed = int(renormalize(clamped_temp, [minTemp, maxTemp], [minSpeed, maxSpeed]))

        # Store the current values in the history
        temp_history.append(raw_temp)
        fan_history.append(fan_speed)
        
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
