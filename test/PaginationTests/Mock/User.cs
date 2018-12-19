using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PaginationTests.Mock {
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Uid { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        [StringLength(50)]
        public string UserName { get; set; }

        /// <summary>
        /// User Full Name
        /// </summary>
        [StringLength(255)]
        public string FullName { get; set; }

        /// <summary>
        /// Created Date
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}