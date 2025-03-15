using System;

namespace Billy.Users.Domain
{
    public static class StringExtensions
    {
        public static string ReverseString( string s )
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}