using DataTableTutorial.Hubs;
using DataTableTutorial.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DataTableTutorial.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<MainHub> _mainHub;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, IHubContext<MainHub> mainHub, IConfiguration configuration, ILogger<HomeController> logger)
        {
            _context = context;
            _mainHub = mainHub;
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetData()
        {
            var data = GetAll();
            return Ok(data);
        }

        [HttpPost]
        public DTResponse GetAll()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            var dataList = new List<Models.DataTableTutorial>();

            var recordsFiltered = 0;



            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault(); // correct
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1.  create a command object identifying the stored procedure
                SqlCommand cmd = new SqlCommand("spGetDataList", conn);

                // 2. set the command object so it knows to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // 3. add parameter to command, which will be passed to the stored procedure
                cmd.Parameters.Add(new SqlParameter("@SearchValue", searchValue));
                cmd.Parameters.Add(new SqlParameter("@PageNo", start));
                cmd.Parameters.Add(new SqlParameter("@PageSize", pageSize));
                cmd.Parameters.Add(new SqlParameter("@SortColumn", 0));
                cmd.Parameters.Add(new SqlParameter("@SortDirection", sortColumnDirection));



                // execute the command
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
      

                 

                    // iterate through results, printing each to console
                    while (rdr.Read())
                    {
                        recordsFiltered = rdr.GetInt32(3);
                        recordsTotal = rdr.GetInt32(4);

                        dataList.Add(item: new Models.DataTableTutorial
                        {
                            Id = rdr.GetInt32(0),
                            Name = rdr["Name"].ToString(),
                            Status = rdr.GetInt32(2)
                        });
                    }
                }
            }

            DTResponse jsonData = new DTResponse()
            { 
                draw = draw, 
                recordsFiltered = 1000, 
                recordsTotal = recordsTotal, 
                data = dataList 
            };

            return jsonData;
        }

    public IEnumerable<Models.DataTableTutorial> GetAll2()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            var dataList = new List<Models.DataTableTutorial>();

            SqlConnection con = new SqlConnection(connectionString);

            using (var cmd = new SqlCommand(@"SELECT [Id], [Name], [Status] FROM [dbo].[DataTableTutorials]", con))
            {
                SqlDataAdapter da = new(cmd);

                SqlDependency.Start(connectionString);
                var dependency = new SqlDependency(cmd);
                dependency.OnChange += new OnChangeEventHandler(dependency_onChange);

                DataSet ds = new();
                da.Fill(ds);

                for(int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    dataList.Add(item: new Models.DataTableTutorial
                    {
                        Id = int.Parse(ds.Tables[0].Rows[i][0].ToString()),
                        Name = ds.Tables[0].Rows[i][1].ToString(),
                        Status = int.Parse(ds.Tables[0].Rows[i][2].ToString())
                    });
                }
            }

            return dataList;
        }

        private void dependency_onChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                _mainHub.Clients.All.SendAsync("LoadDataTrigger");
            }
        }
    }
}
