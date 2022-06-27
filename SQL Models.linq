<Query Kind="Program">
  <Connection>
    <ID>fa99c967-bc20-4973-843c-99358d728125</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>(local)</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <SqlSecurity>true</SqlSecurity>
    <UserName>sa</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAL89HSzj4PE6YwRzx00HieAAAAAACAAAAAAAQZgAAAAEAACAAAAAH7Jp0jzp++t7FmLOz32WFYyFfo87vvgFa2Q55xCcuigAAAAAOgAAAAAIAACAAAAAX1XQLpHJiUxRpLOXzYbDSf41kWGHfx6vYfMyoryMCRRAAAAAHQLr4SpL6ZolA//CGMajTQAAAAGfDsr0lRQcjawXukVIySNk8iKtCBAQ0ikwWgdY3/kMTvq/uEJuaeKYXKVcVgLZqfz0KXht0Xz4hC53EIzzrAsg=</Password>
    <Database>LoanCar</Database>
  </Connection>
</Query>

void Main()
{
	// 資料表名稱
	var nameOfTableAndClass = "TableName";
	// 這邊修改為您要執行的 SQL Command
	//var sqlCommand = $@"SELECT * FROM {nameOfTableAndClass}";
	var sqlCommand = $@"
SELECT TOP 1 *
FROM TableName
	";
	// 在 DumpClass 方法裡放 SQL Command 和 Class 名稱
	this.Connection.DumpClass(sqlCommand.ToString(), nameOfTableAndClass).Dump();
}

public static class LINQPadExtensions
{
	private static readonly Dictionary<Type, string> TypeAliases = new Dictionary<Type, string> {
		{ typeof(int), "int" },
		{ typeof(short), "short" },
		{ typeof(byte), "byte" },
		{ typeof(byte[]), "byte[]" },
		{ typeof(long), "long" },
		{ typeof(double), "double" },
		{ typeof(decimal), "decimal" },
		{ typeof(float), "float" },
		{ typeof(bool), "bool" },
		{ typeof(string), "string" }
	};

	private static readonly HashSet<Type> NullableTypes = new HashSet<Type> {
		typeof(int),
		typeof(short),
		typeof(long),
		typeof(double),
		typeof(decimal),
		typeof(float),
		typeof(bool),
		typeof(DateTime)
	};

	public static string DumpClass(this IDbConnection connection, string sql, string className = "Info")
	{
		if (connection.State != ConnectionState.Open)
		{
			connection.Open();
		}

		var cmd = connection.CreateCommand();
		cmd.CommandText = sql;
		var reader = cmd.ExecuteReader();

		var builder = new StringBuilder();
		do
		{
			if (reader.FieldCount <= 1) continue;

			builder.AppendFormat("public class {0}{1}", className, Environment.NewLine);
			builder.AppendLine("{");
			var schema = reader.GetSchemaTable();

			foreach (DataRow row in schema.Rows)
			{
				var type = (Type)row["DataType"];
				var name = TypeAliases.ContainsKey(type) ? TypeAliases[type] : type.Name;
				var isNullable = (bool)row["AllowDBNull"] && NullableTypes.Contains(type);
				var collumnName = (string)row["ColumnName"];
				
				if(type == typeof(string)){
					builder.AppendLine(string.Format("\t[StringLength({0})]", row["ColumnSize"]));
				}
				
				builder.AppendLine(string.Format("\tpublic {0}{1} {2} {{ get; set; }}", name, isNullable ? "?" : string.Empty, collumnName));
				builder.AppendLine();
			}

			builder.AppendLine("}");
			builder.AppendLine();
		} while (reader.NextResult());

		return builder.ToString();
	}
}