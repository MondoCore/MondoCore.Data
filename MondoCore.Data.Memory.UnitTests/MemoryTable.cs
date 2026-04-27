using System.Text.Json.Serialization;

using System.Linq;

using MondoCore.Data.Memory;

namespace MondoCore.Data.UnitTests
{
    [TestClass]
    [TestCategory("Unit Tests")]
    public class MemoryTableTests 
    {
        private ITable<string, Automobile> _table = new MemoryTable<string, Automobile>();

        private List<string> _idCollection = new List<string>();

        public MemoryTableTests()
        {
        }

        [TestInitialize]
        public async Task Initialize()
        {
            await _table!.Writer.Delete( _=> true );

            _idCollection.Clear();

            for(var i = 0; i < 6; ++i)
                _idCollection.Add(Guid.NewGuid().ToString());

            await _table.Writer.Insert(new Automobile { Id = _idCollection[0], Make = "Chevy",      Color = "Blue",  Model = "Camaro",    Year = 1969 });
            await _table.Writer.Insert(new Automobile { Id = _idCollection[1], Make = "Pontiac",    Color = "Black", Model = "Firebird",  Year = 1972 });
            await _table.Writer.Insert(new Automobile { Id = _idCollection[2], Make = "Chevy",      Color = "Green", Model = "Corvette",  Year = 1964 });
            await _table.Writer.Insert(new Automobile { Id = _idCollection[3], Make = "Audi",       Color = "Blue",  Model = "S5",        Year = 2021 });
            await _table.Writer.Insert(new Automobile { Id = _idCollection[4], Make = "Studebaker", Color = "Black", Model = "Speedster", Year = 1914 });
            await _table.Writer.Insert(new Automobile { Id = _idCollection[5], Make = "Arrow",      Color = "Green", Model = "Glow",      Year = 1917 });
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _table!.Writer.Delete( _=> true );

            _idCollection.Clear();
        }

        [TestMethod]
        public async Task Writer_Insert()
        {
            var id = Guid.NewGuid().ToString();

            await _table.Writer.Insert(new Automobile 
            {
               Id = id,
               Make = "Chevy",
               Model = "GTO",
               Color = "Dark Blue",
               Year = 1972
            });

            var result = await _table.Reader.Get(id);

            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual("GTO", result.Model);

            await _table.Writer.Delete(id);
        }
        [TestMethod]
        public async Task Writer_Insert_many()
        {
            var id1 = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();

            await _table.Writer.Insert(new List<Automobile> 
            {
                new Automobile 
                {
                   Id = id1,
                   Make = "Pontiac",
                   Model = "GTO",
                   Color = "Dark Blue",
                   Year = 1972
                },
                new Automobile 
                {
                   Id = id2,
                   Make = "Aston-Martin",
                   Model = "DB9",
                   Color = "Cobalt",
                   Year = 1968
                }
            });

            var result1 = await _table.Reader.Get(id1);

            Assert.IsNotNull(result1);
            Assert.AreEqual(id1, result1.Id);
            Assert.AreEqual("GTO", result1.Model);

            var result2 = await _table.Reader.Get(id2);

            Assert.IsNotNull(result2);
            Assert.AreEqual(id2, result2.Id);
            Assert.AreEqual("DB9", result2.Model);
        }

        #region Get

        [TestMethod]
        public async Task Reader_Get_success()
        {
            var id = Guid.NewGuid().ToString();
        
            await _table.Writer.Insert(new Automobile { Id = id, Make = "Chevy", Model = "Camaro" });

            Assert.IsNotNull(await _table.Reader.Get(id));
        }

        [TestMethod]
        public async Task Reader_Get_notfound()
        {
            var id = Guid.NewGuid().ToString();
            
            await _table.Writer.Insert(new Automobile { Id = id, Make = "Chevy", Model = "Camaro" });

            await Assert.ThrowsAsync<NotFoundException>( async ()=> await _table.Reader.Get(Guid.NewGuid().ToString()));
        }
        
        [TestMethod]
        public async Task Reader_Get_wExpression()
        {
            var result = await _table.Reader.Get( o=> o.Make == "Chevy").ToListAsync();

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Where( c=> c.Model == "Corvette").Any());
            Assert.IsTrue(result.Where( c=> c.Model == "Camaro").Any());

            var result2 = await _table.Reader.Get( o=> o.Year < 1970).ToListAsync();

            Assert.AreEqual(4, result2.Count());
        }
       
        [TestMethod]
        public async Task Reader_Get_wId_list()
        {
            var id1 = _idCollection[1];
            var id2 = _idCollection[2];

            var result = await _table.Reader.Get( new List<string> { id1, id2 } ).ToListAsync();

            Assert.AreEqual(2, result.Count());

            Assert.IsTrue(result.Where( c=> c.Model == "Corvette").Any());
            Assert.IsTrue(result.Where( c=> c.Model == "Firebird").Any());
        }

        [TestMethod]
        public async Task Reader_Get_wExpression_notfound()
        {
            var result = await _table.Reader.Get( o=> o.Make == "Chevy").ToListAsync();

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Where( c=> c.Model == "Corvette").Any());
            Assert.IsTrue(result.Where( c=> c.Model == "Camaro").Any());

            var result2 = await _table.Reader.Get( o=> o.Year < 1900).ToListAsync();

            Assert.AreEqual(0, result2.Count);
        }
        
        #endregion

        [TestMethod]
        public async Task Writer_Update()
        {
            Assert.IsTrue(await _table.Writer.Update(new Automobile { Id = _idCollection[0], Make = "Chevy", Model = "Camaro", Year = 1970 }));  

            var result = await _table.Reader.Get(_idCollection[0]);

            Assert.AreEqual(1970, result.Year);
        }

        [TestMethod]
        public async Task Writer_Update_wGuard_succeeds()
        {
            var result = await _table.Writer.Update(new Automobile { Id = _idCollection[0], Make = "Chevy", Model = "Camaro", Color = "Blue", Year = 1970 }, (i)=> i.Color == "Blue");  

            Assert.IsTrue(result);  

            var result1 = await _table.Reader.Get(_idCollection[0]);
            var result2 = await _table.Reader.Get(_idCollection[1]);
            var result3 = await _table.Reader.Get(_idCollection[2]); 
            var result4 = await _table.Reader.Get(_idCollection[3]);

            Assert.AreEqual(1970, result1.Year);
            Assert.AreEqual(1972, result2.Year);
            Assert.AreEqual(1964, result3.Year);
            Assert.AreEqual(2021, result4.Year);
        }

        [TestMethod]
        public async Task Writer_Update_wGuard_fails()
        {
            Assert.IsFalse(await _table.Writer.Update(new Automobile { Id = _idCollection[0], Make = "Chevy", Model = "Camaro", Year = 1970 }, (i)=> i.Color == "Periwinkle"));  

            var result1 = await _table.Reader.Get(_idCollection[0]);
            var result2 = await _table.Reader.Get(_idCollection[1]);
            var result3 = await _table.Reader.Get(_idCollection[2]);
            var result4 = await _table.Reader.Get(_idCollection[3]);


            Assert.AreEqual(1969, result1.Year);
            Assert.AreEqual(1972, result2.Year);
            Assert.AreEqual(1964, result3.Year);
            Assert.AreEqual(2021, result4.Year);
        }
        
        [TestMethod]
        public async Task Writer_Update_properties_succeeds()
        {
            Assert.AreEqual(2, await _table.Writer.Update(new { Year = 1970 }, (i)=> i.Color == "Blue"));  

            var result1 = await _table.Reader.Get(_idCollection[0]);
            var result2 = await _table.Reader.Get(_idCollection[1]);
            var result3 = await _table.Reader.Get(_idCollection[2]);
            var result4 = await _table.Reader.Get(_idCollection[3]);
            var result5 = await _table.Reader.Get(_idCollection[4]);
            var result6 = await _table.Reader.Get(_idCollection[5]);

            Assert.AreEqual(1970, result1.Year);
            Assert.AreEqual(1972, result2.Year);
            Assert.AreEqual(1964, result3.Year);
            Assert.AreEqual(1970, result4.Year);
            Assert.AreEqual(1914, result5.Year);
            Assert.AreEqual(1917, result6.Year);
        }

        [TestMethod]
        public async Task Writer_Update_properties_2vals_succeeds()
        {
            Assert.AreEqual(2, await _table.Writer.Update(new { Year = 1970, Color = "Red" }, (i)=> i.Color == "Blue"));  

            var result1 = await _table.Reader.Get(_idCollection[0]);
            var result2 = await _table.Reader.Get(_idCollection[1]);
            var result3 = await _table.Reader.Get(_idCollection[2]);
            var result4 = await _table.Reader.Get(_idCollection[3]);
            var result5 = await _table.Reader.Get(_idCollection[4]);
            var result6 = await _table.Reader.Get(_idCollection[5]);

            Assert.AreEqual(1970, result1.Year);
            Assert.AreEqual(1972, result2.Year);
            Assert.AreEqual(1964, result3.Year);
            Assert.AreEqual(1970, result4.Year);
            Assert.AreEqual(1914, result5.Year);
            Assert.AreEqual(1917, result6.Year);

            Assert.AreEqual("Red",   result1.Color);
            Assert.AreEqual("Black", result2.Color);
            Assert.AreEqual("Green", result3.Color);
            Assert.AreEqual("Red",   result4.Color);
            Assert.AreEqual("Black", result5.Color);
            Assert.AreEqual("Green", result6.Color);
        }

        [TestMethod]
        public async Task Writer_Update_lambda_succeeds()
        {
            Assert.AreEqual(2, await _table.Writer.Update((i)=> 
            {
                i.Year = 1970;

                return Task.FromResult((true, true));
            },
            (i)=> i.Color == "Blue"));  

            var result1 = await _table.Reader.Get(_idCollection[0]);
            var result2 = await _table.Reader.Get(_idCollection[1]);
            var result3 = await _table.Reader.Get(_idCollection[2]);
            var result4 = await _table.Reader.Get(_idCollection[3]);
            var result5 = await _table.Reader.Get(_idCollection[4]);
            var result6 = await _table.Reader.Get(_idCollection[5]);

            Assert.AreEqual(1970, result1.Year);
            Assert.AreEqual(1972, result2.Year);
            Assert.AreEqual(1964, result3.Year);
            Assert.AreEqual(1970, result4.Year);
            Assert.AreEqual(1914, result5.Year);
            Assert.AreEqual(1917, result6.Year);
        }
    }
}
