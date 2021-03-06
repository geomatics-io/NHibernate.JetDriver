using System;
using System.Data;
using NHibernate.Dialect.Schema;

namespace NHibernate.JetDriver.Schema
{
    public class JetForeignKeyMetadata : AbstractForeignKeyMetadata
    {
        public JetForeignKeyMetadata(DataRow rs)
            : base(rs)
        {
            Name = Convert.ToString(rs["FK_NAME"]);
        }
    }
}