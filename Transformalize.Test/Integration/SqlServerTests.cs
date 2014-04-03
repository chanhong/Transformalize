﻿#region License

// /*
// Transformalize - Replicate, Transform, and Denormalize Your Data...
// Copyright (C) 2013 Dale Newman
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// */

#endregion

using NUnit.Framework;
using Transformalize.Configuration.Builders;
using Transformalize.Main;

namespace Transformalize.Test.Integration {

    [TestFixture]
    public class SqlServerTests {

        [Test]
        public void TestInit() {

            var config = new ProcessBuilder("SqlServerTest")
                .Connection("input").Database("Orchard171")
                .Connection("output").Provider("console")
                .Entity("SS_BatchRecord")
                    .SqlKeysOverride("SELECT ActionValue AS TflKey FROM SS_BatchRecord WHERE BatchId = 7;")
                    .Field("TflKey").Int32().PrimaryKey()
                .Process();

            ProcessFactory.Create(config).Run();
        }

    }
}