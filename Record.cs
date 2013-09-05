
using System;
using System.Collections.Generic;

namespace krsclient.net
{
    class Record
    {
        private readonly string _offsetToken;
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
