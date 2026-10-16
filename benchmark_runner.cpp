#include <iostream>
#include <vector>
#include <chrono>
#include <iomanip>
#include <fstream>

using namespace std;
using namespace std::chrono;

// =========================================================================
// 1. PLAIN BACKTRACKING SOLVER
// =========================================================================
bool isSatisfyPlain(int row, int col, int n, 
                    const vector<bool>& cols, 
                    const vector<bool>& diag1, 
                    const vector<bool>& diag2) {
    int d1 = row - col + n - 1; // Đường chéo chính (\)
    int d2 = row + col;         // Đường chéo phụ (/)
    return (!cols[col] && !diag1[d1] && !diag2[d2]);
}

void solvePlain(int row, int n, vector<bool>& cols, vector<bool>& diag1, vector<bool>& diag2, 
                vector<int>& board, long long& solutions, long long& nodes) {
    nodes++;

    if (row == n) {
        solutions++;
        return;
    }

    for (int col = 0; col < n; col++) {
        if (isSatisfyPlain(row, col, n, cols, diag1, diag2)) {
            int d1 = row - col + n - 1;
            int d2 = row + col;

            board[row] = col;
            cols[col] = diag1[d1] = diag2[d2] = true;

            solvePlain(row + 1, n, cols, diag1, diag2, board, solutions, nodes);

            cols[col] = diag1[d1] = diag2[d2] = false;
        }
    }
}

// =========================================================================
// 2. BITMASK BACKTRACKING SOLVER (Knuth's Algorithm)
// =========================================================================
bool isSatisfyBitmask(int bit, int colmask, int ld, int rd) {
    return ((colmask | ld | rd) & bit) == 0;
}

void solveBitmask(int row, int ld, int rd, int colmask, int n, 
                  long long& solutions, long long& nodes) {
    nodes++;

    int all_ones = (1 << n) - 1;
    if (colmask == all_ones) {
        solutions++;
        return;
    }

    int available = ~(colmask | ld | rd) & all_ones;

    while (available) {
        int bit = available & -available;
        available -= bit;

        if (isSatisfyBitmask(bit, colmask, ld, rd)) {
            solveBitmask(row + 1, (ld | bit) << 1, (rd | bit) >> 1, colmask | bit, n, solutions, nodes);
        }
    }
}

// Cấu trúc lưu số liệu đo đạc thực nghiệm
struct BenchmarkItem {
    int n;
    long long solutions;
    long long nodes;
    double timePlain;
    double timeBitmask;
    double speedup;
};

// =========================================================================
// 3. MAIN RUNNER: ĐO ĐẠC VÀ XUẤT RA FILE benchmark_result.txt
// =========================================================================
int main() {
    ios_base::sync_with_stdio(false);
    cin.tie(nullptr);

    cout << "=========================================================================\n";
    cout << "   RUNNER THUC NGHIEM: KHAO SAT N = 8, 10, 12, 14                       \n";
    cout << "=========================================================================\n";

    vector<int> test_sizes = {8, 10, 12, 14};
    vector<BenchmarkItem> tableData;

    for (int n : test_sizes) {
        cout << "Dang tien hanh do N = " << n << " ... " << flush;

        // 1. Đo Plain Backtracking
        long long s_plain = 0, nd_plain = 0;
        vector<bool> cols(n, false), diag1(2 * n, false), diag2(2 * n, false);
        vector<int> board(n, 0);

        auto t1 = high_resolution_clock::now();
        solvePlain(0, n, cols, diag1, diag2, board, s_plain, nd_plain);
        auto t2 = high_resolution_clock::now();
        double time_plain = duration<double>(t2 - t1).count();

        // 2. Đo Bitmask Backtracking
        long long s_bit = 0, nd_bit = 0;
        auto t3 = high_resolution_clock::now();
        solveBitmask(0, 0, 0, 0, n, s_bit, nd_bit);
        auto t4 = high_resolution_clock::now();
        double time_bitmask = duration<double>(t4 - t3).count();

        double speedup = (time_bitmask > 1e-7) ? (time_plain / time_bitmask) : 1.0;

        tableData.push_back({n, s_plain, nd_plain, time_plain, time_bitmask, speedup});
        cout << "Xong! (Speedup: " << fixed << setprecision(2) << speedup << "x)\n";
    }

    // Xuất bảng số liệu thô vào file benchmark_result.txt
    ofstream outFile("benchmark_result.txt");
    if (!outFile.is_open()) {
        cerr << "Loi: Khong the tao file benchmark_result.txt!\n";
        return 1;
    }

    outFile << "====================================================================================================\n";
    outFile << "                        KET QUA DO THUC NGHIEM (RAW BENCHMARK DATA)\n";
    outFile << "                        N-QUEENS: PLAIN BACKTRACKING VS BITMASK (KNUTH)\n";
    outFile << "====================================================================================================\n";
    outFile << "Moi truong chay      : benchmark_runner.cpp (Bien dich voi g++ -O3 -std=c++11)\n";
    outFile << "Cac moc khao sat     : N = 8, 10, 12, 14\n";
    outFile << "====================================================================================================\n\n";

    outFile << left << setw(6)  << "N"
            << setw(16) << "So nghiem"
            << setw(18) << "So node da duyet"
            << setw(24) << "Thoi gian Plain (s)"
            << setw(24) << "Thoi gian Bitmask (s)"
            << "Toc do tang (Speedup)\n";
    outFile << "----------------------------------------------------------------------------------------------------\n";

    for (const auto& row : tableData) {
        outFile << left << setw(6)  << row.n
                << setw(16) << row.solutions
                << setw(18) << row.nodes
                << setw(24) << fixed << setprecision(6) << row.timePlain
                << setw(24) << fixed << setprecision(6) << row.timeBitmask
                << fixed << setprecision(2) << row.speedup << "x\n";
    }
    outFile << "----------------------------------------------------------------------------------------------------\n";

    outFile.close();
    cout << "\n>>> DA XUAT BANG SO LIEU THO VAO FILE benchmark_result.txt! <<<\n";
    return 0;
}