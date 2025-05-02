# TimeSyncXp

This is a simple Visual Basic .NET console application to synchronize your Windows system time with an NTP server. I made it with the help of Chat GPT. It is a relatively simple program for Windows XP, but it works with newer versions as well.

## Features

- Fetches the current time from `pool.ntp.org`.
- Sets the local system time using Windows API.
- Checks for Administrator privileges before updating time.
- Designed to work on Windows XP and later.

## Requirements

- .NET Framework 3.5 or compatible.
- Administrative privileges to set the system clock.
- Internet connection to reach the NTP server.

## Usage

1. **Build the project** using Visual Studio 2008 (or later).
2. **Run the application as Administrator.**
3. The app will fetch the current NTP time and update your system clock.
4. The console will display a success or error message and wait for you to press any key before closing.

## Notes

- If you run without Administrator privileges, the program will give you an error.

## License

This project is open source and available under the MIT License.
