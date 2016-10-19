// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileIdentifier.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Models {
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    /// <summary>
    /// Represents the identifier of an uploaded file
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct FileIdentifier : IEquatable<FileIdentifier> {
        private static int Mask = ~1;

        private readonly Guid _id;
        private string _stringRepresentation;

        private FileIdentifier(Guid id) : this() {
            this._id = id;
        }

        private FileIdentifier(string stringRepresentation) : this() {
            ValidateStringRepresentation(stringRepresentation);

            this._stringRepresentation = stringRepresentation;
        }

        private static void ValidateStringRepresentation(string stringRepresentation) {
            if (stringRepresentation == null) throw new ArgumentNullException(nameof(stringRepresentation));

            foreach (char ch in stringRepresentation) {
                if (Array.IndexOf(Chars, ch) == -1) {
                    throw new ArgumentException($"Unexpected character '{ch}' in input", nameof(stringRepresentation));
                }
            }
        }

        /// <summary>
        /// Parses a <see cref="FileIdentifier"/> from a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static FileIdentifier FromString(string str) {
            return new FileIdentifier(str);
        }

        /// <summary>
        /// Creates a new log message id
        /// </summary>
        /// <returns></returns>
        public static FileIdentifier CreateNew() {
            Interlocked.Increment(ref Mask);
            return new FileIdentifier(Guid.NewGuid());
        }


        /// <summary>
        /// Returns the string representation of this file identifier
        /// </summary>
        public override string ToString() {
            return this._stringRepresentation ?? (this._stringRepresentation = MakeStringRepresentation(this._id));
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(FileIdentifier other) {
            if (this._stringRepresentation == null && other._stringRepresentation == null) {
                return other._id == this._id;
            }

            if (this._stringRepresentation == null) {
                string thisStr = MakeStringRepresentation(this._id);
                string otherStr = other._stringRepresentation;

                return string.Equals(thisStr, otherStr, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(this._stringRepresentation, other._stringRepresentation, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param>
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is FileIdentifier && this.Equals((FileIdentifier)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() {
            string strVal = this.ToString();
            return StringComparer.OrdinalIgnoreCase.GetHashCode(strVal);
        }

        /// <summary>
        /// Checks equality of the given parameters
        /// </summary>
        public static bool operator ==(FileIdentifier left, FileIdentifier right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks inequality of the given parameters
        /// </summary>
        public static bool operator !=(FileIdentifier left, FileIdentifier right) {
            return !left.Equals(right);
        }

        private static readonly char[] Chars = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '2', '3', '4', '5', '6', '7', '8', '9'
        };

        private static unsafe string MakeStringRepresentation(Guid id) {
            // Convert Guid to bytes
            GuidBuffer buffer = new GuidBuffer(id);
            byte* bytes = buffer.buffer;

            // Target string
            int size = sizeof(Guid) * 2;
            char* result = stackalloc char[size + 1 /* \0 terminator */];
            char* start = result;

            for (int i = 0; i < sizeof(Guid); i++) {
                byte src = *bytes;
                int carry = 0;

                {
                    int index = src % Chars.Length;

                    char current = Chars[index];

                    *result = current;
                    result++;

                    carry = ((src - index) / Chars.Length) - 1;
                }

                if (carry > 0) {
                    int index = carry % Chars.Length;

                    char current = Chars[index];

                    *result = current;
                    result++;
                }

                bytes++;
            }

            return new string(start);
        }

        [StructLayout(LayoutKind.Explicit)]
        unsafe struct GuidBuffer {
            [FieldOffset(0)]
            public fixed byte buffer[16];

            [FieldOffset(0)]
            public Guid Guid;

            public GuidBuffer(Guid guid) : this() {
                this.Guid = guid;
            }
        }
    }
}
