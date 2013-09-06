
using System;
using System.Collections.Generic;

namespace krsclient.net
{
    /// <summary>
    /// En række modtaget fra kopi register servicen
    /// </summary>
    class Record
    {
        /// <summary>
        /// Offset token for denne række
        /// </summary>
        private readonly string _offsetToken;

        /// <summary>
        /// Felt navne og værdier
        /// </summary>
        private readonly SortedList<String, String> _fieldValues;

        public Record(String offsetToken, SortedList<String, String> fieldValues)
        {
            _offsetToken = offsetToken;
            _fieldValues = fieldValues;
        }

        public SortedList<String, String> FieldValues
        {
            get { return _fieldValues; }
        }

        public string OffsetToken
        {
            get { return _offsetToken; }
        }

        public String ValueOfFieldNamed(string name)
        {
            return FieldValues.ContainsKey(name) ? FieldValues[name] : null;
        }

        public override string ToString()
        {
            String result = "Record ("+OffsetToken+"): ";
            foreach (KeyValuePair<String, String> fieldValue in FieldValues)
            {
                result += "<" + fieldValue.Key + " = " + fieldValue.Value + ">";
            }
            return result;
        }
    }
}
