using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lingya.Pagination.Tests.Mock;
public class User {
    static int globalId = 0;
    public User() { }

    public User(string userName, string fullName) {
        Uid = globalId++;
        UserName = userName;
        FullName = fullName;
        CreatedDate = DateTime.Now;
    }

    public User(int uid, string userName, string fullName, DateTime createdDate) {
        Uid = uid;
        UserName = userName;
        FullName = fullName;
        CreatedDate = createdDate;
    }

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

    [StringLength(255)]
    public string DepName { get; set; }

    /// <summary>
    /// Created Date
    /// </summary>
    public DateTime CreatedDate { get; set; }
}