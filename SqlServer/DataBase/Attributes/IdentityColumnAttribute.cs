using System;

namespace SqlServer.Attributes
{
    public class IdentityColumnAttribute : Attribute
    {
        bool isIdentity;
        public IdentityColumnAttribute(bool value = true)
        {
            isIdentity = value;
        }
    }
}