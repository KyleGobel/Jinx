using System;
using System.Data;
using System.Dynamic;

namespace Jinx.Services
{
    public class DynamicDataReader : DynamicObject
    {
        private IDataReader _dataReader;

        public DynamicDataReader(IDataReader dataReader)
        {
            _dataReader = dataReader;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            if (binder.Name == "IsClosed")
            {
                result = _dataReader.IsClosed;
            }
            else if (binder.Name == "RecordsAffected")
            {
                result = _dataReader.RecordsAffected;
            }
            else
            {
                try
                {
                    result = _dataReader[binder.Name];
                    if (result == DBNull.Value)
                        result = null;

                }
                catch (Exception)
                {
                    result = null;
                    return false;
                }
            }
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder.Name == "Read")
                result = _dataReader.Read();
            else if (binder.Name == "Close")
            {
                _dataReader.Close();
                result = null;
            }
            else
            {
                //no other methods supporte dyet
                result = null;
                return false;
            }
            return true;
        }

    }
}