Public Class DataUtil
    Public Shared Function ReadInt(data As Byte(), offset As Integer) As Integer
        Return CInt(data(offset)) + CInt(data(offset + 1)) * &H100 + CInt(data(offset + 2)) * &H10000 + CInt(data(offset + 3)) * &H1000000
    End Function
End Class
