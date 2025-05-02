Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.Threading

Module TimeSync

    ' Corrected DllImport with line continuation
    <DllImport("kernel32.dll", SetLastError:=True)> _
    Private Function SetSystemTime(ByRef st As SYSTEMTIME) As Boolean
    End Function

    ' Use line continuation for the StructLayout attribute
    <StructLayout(LayoutKind.Sequential)> _
    Public Structure SYSTEMTIME
        Public wYear As Short
        Public wMonth As Short
        Public wDayOfWeek As Short
        Public wDay As Short
        Public wHour As Short
        Public wMinute As Short
        Public wSecond As Short
        Public wMilliseconds As Short
    End Structure

    Sub Main()

        If Not IsRunningAsAdmin() Then
            Console.WriteLine("This program requires administrator privileges.")
            Console.WriteLine("Press any key to exit...")
            Console.ReadKey() ' Wait for key press before exiting
            Return
        End If

        Try
            ' Check if running as Administrator
            Dim user As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
            Dim principal As New System.Security.Principal.WindowsPrincipal(user)

            If Not principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator) Then
                Console.WriteLine("This application must be run as Administrator to set the system time.")
                Return
            End If

            ' Prepare NTP request
            Dim ntpData(47) As Byte
            ntpData(0) = &H1B

            ' Get IPv4 address for NTP server (filter out IPv6)
            Dim addresses = Dns.GetHostEntry("pool.ntp.org").AddressList
            Dim ipv4Address As IPAddress = Nothing

            For Each addr As IPAddress In addresses
                If addr.AddressFamily = AddressFamily.InterNetwork Then ' IPv4
                    ipv4Address = addr
                    Exit For
                End If
            Next

            If ipv4Address Is Nothing Then
                Throw New Exception("No IPv4 address found for pool.ntp.org")
            End If

            Dim ipEndPoint = New IPEndPoint(ipv4Address, 123)
            Dim socket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)

            ' Connect to the NTP server and get the time
            socket.Connect(ipEndPoint)
            socket.Send(ntpData)
            socket.Receive(ntpData)
            socket.Close()

            ' Process NTP response
            Dim intPart As UInteger = BitConverter.ToUInt32(ntpData, 40)
            Dim fractPart As UInteger = BitConverter.ToUInt32(ntpData, 44)

            intPart = SwapEndianness(intPart)
            fractPart = SwapEndianness(fractPart)

            Dim milliseconds As ULong = (CLng(intPart) * 1000) + ((CLng(fractPart) * 1000) \ &H100000000UL)
            Dim networkDateTime As DateTime = New DateTime(1900, 1, 1).AddMilliseconds(CType(milliseconds, Double))

            ' Convert to local time
            'networkDateTime = networkDateTime.ToLocalTime()

            ' Set system time
            Dim st As New SYSTEMTIME()
            st.wYear = CShort(networkDateTime.Year)
            st.wMonth = CShort(networkDateTime.Month)
            st.wDay = CShort(networkDateTime.Day)
            st.wHour = CShort(networkDateTime.Hour)
            st.wMinute = CShort(networkDateTime.Minute)
            st.wSecond = CShort(networkDateTime.Second)
            st.wMilliseconds = CShort(networkDateTime.Millisecond)

            ' Update the system time
            If SetSystemTime(st) Then
                Console.WriteLine("System time updated successfully!")
            Else
                Console.WriteLine("Failed to set system time. Run as administrator?")
            End If

            ' Wait before closing the console window
            Console.WriteLine("Press any key to exit...")
            Console.ReadKey()

        Catch ex As Exception
            Console.WriteLine("Error: " & ex.Message)
        End Try
    End Sub

    Private Function IsRunningAsAdmin() As Boolean
        Dim currentIdentity As WindowsIdentity = WindowsIdentity.GetCurrent()
        Dim principal As New WindowsPrincipal(currentIdentity)
        Return principal.IsInRole(WindowsBuiltInRole.Administrator)
    End Function

    Private Function SwapEndianness(ByVal x As UInteger) As UInteger
        Return ((x And &HFF000000UI) \ &H1000000UI) Or _
               ((x And &HFF0000UI) \ &H100UI) Or _
               ((x And &HFF00UI) * &H100UI) Or _
               ((x And &HFFUI) * &H1000000UI)
    End Function

End Module
