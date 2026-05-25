using System.Data;
using Zoapex.DataAccess;

namespace Zoapex.Business;

public class CategoryBL
{
    private readonly CategoryDAL _dal = new();

    public DataTable GetAllCategories() => _dal.GetAllCategories();
}
