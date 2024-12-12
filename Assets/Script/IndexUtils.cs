namespace IndexUtils
{
    public static class Converter
    {
        // Converte (linha, coluna) em índice linear
        public static int Multi2Uni((int row, int column) index, (int row, int column) limit) =>
            index.column + index.row * limit.column;

        // Converte índice linear em (linha, coluna)
        public static (int row, int column) Uni2Multi(int index, (int row, int column) limit) =>
            (index / limit.column, index % limit.column);
    }
}
