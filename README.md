# Lucene.NET SQL

Lucene.NET SQL is a library that enables persistance of Lucene index files to SQL databases.

![Master Build](https://github.com/GowenGit/lucene-net-sql/workflows/Master%20Build/badge.svg) ![Deploy](https://github.com/GowenGit/lucene-net-sql/workflows/Deploy/badge.svg) [![NuGet](https://img.shields.io/nuget/v/Lucene.Net.Sql.MySql.svg)](https://www.nuget.org/packages/Lucene.Net.Sql.MySql)

## Supported SQL Engines

* [x] MySQL
* [ ] PostgreSQL
* [ ] SQL Server

## Status

Currently this package is in `alpha` while we wait for `lucene.net 4.8` to be released.

## License

### Commercial license

If you want to use Lucene.NET SQL to develop commercial projects, and applications, the Commercial license is the appropriate license. With this option, your source code is kept proprietary. Purchase a Lucene.NET SQL Commercial License at [gowengit.github.io/lucene-net-sql](https://gowengit.github.io/lucene-net-sql)

### Open source license

If you are creating an open source application under a license compatible with the [GNU GPL license v3](https://www.gnu.org/licenses/gpl-3.0.html), you may use Lucene.NET SQL under the terms of the GPLv3.

## Examples

Below are some general use examples of the library.

### MySQL

```cs
var options = new SqlDirectoryOptions(connectionString, "ExampleDirectory");

using var mySqlOperator = MySqlLuceneOperator.Create(options);

using var directory = new LuceneSqlDirectory(options, mySqlOperator);

// Rest is standard Lucene code

using var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

using var writer = new IndexWriter(directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer));

// ...
```

*Note:* `MySqlLuceneOperator` is linked to a single `MySQL` connection so should be either transient or short lived.

## Performance Benchmarks

Performance benchmarks were done to compare local file system vs local SQL instance performance.

``` ini
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
AMD Ryzen 5 2600, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.401
  [Host]     : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  Job-QKPTDT : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
```

### A run on ten indexed novels line by line, around *7MB* of text data.

|                         Method |      Mean |     Error |    StdDev |    Median |
|------------------------------- |----------:|----------:|----------:|----------:|
|      FuzzySearchMySqlTenNovels | 28.877 ms | 0.4831 ms | 0.4519 ms | 28.771 ms |
| FuzzySearchFileSystemTenNovels |  3.107 ms | 0.0950 ms | 0.2771 ms |  3.046 ms |
|      QuerySearchMySqlTenNovels |  6.424 ms | 0.1187 ms | 0.2554 ms |  6.332 ms |
| QuerySearchFileSystemTenNovels |  1.170 ms | 0.0233 ms | 0.0635 ms |  1.143 ms |

### A run on one hundred indexed novels, around *70MB* of text data.

|                             Method |      Mean |    Error |   StdDev |    Median |
|----------------------------------- |----------:|---------:|---------:|----------:|
|      FuzzySearchMySqlHundredNovels | 290.90 ms | 1.621 ms | 1.354 ms | 290.74 ms |
| FuzzySearchFileSystemHundredNovels |  20.94 ms | 0.288 ms | 0.256 ms |  20.83 ms |
|      QuerySearchMySqlHundredNovels |  52.00 ms | 1.012 ms | 1.082 ms |  51.85 ms |
| QuerySearchFileSystemHundredNovels |  11.44 ms | 0.207 ms | 0.388 ms |  11.24 ms |