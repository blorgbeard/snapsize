using System.Collections.Generic;

namespace Snapsize
{
    static class DefaultSnapAreas
    {
        public static string Value = @"
# each area is in order: top, left, bottom, right
# each coordinate is expressed in fractions of the screen

# horizontal halves
0, 0, 1/2, 1
1/2, 0, 1, 1

# horizontal thirds
0, 0, 1/3, 1
1/3, 0, 2/3, 1
2/3, 0, 1, 1

# horizontal 1/3 and 2/3
0, 0, 1/3, 1
1/3, 0, 1, 1

# horizontal 2/3 and 1/3
0, 0, 2/3, 1
2/3, 0, 1, 1

# quadrants
0, 0, 1/2, 1/2
1/2, 0, 1, 1/2
0, 1/2, 1/2, 1
1/2, 1/2, 1, 1
";

    }
}
