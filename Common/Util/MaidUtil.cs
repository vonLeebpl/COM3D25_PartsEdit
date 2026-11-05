using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal static class MaidUtil {
    public static string GetMaidFullName(Maid maid) {
#if COM3D2
        return maid.status.lastName + " " + maid.status.firstName;
#else
        return maid.Param.status.last_name + " " + maid.Param.status.first_name;
#endif
    }
}
