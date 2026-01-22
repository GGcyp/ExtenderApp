using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件对话框服务（UI 层实现）
    /// 提供打开文件/保存文件对话框的抽象，便于在 ViewModel 中注入并单元测试替换实现。
    /// </summary>
    public interface IFileDialogService
    {
        /// <summary>
        /// 打开“打开文件”对话框，返回所选文件完整路径集合；若取消选择返回 null 。
        /// </summary>
        /// <param name="title">对话框标题，可为 null 使用默认。</param>
        /// <param name="filter">文件筛选器，例如 "视频文件|*.mp4;*.mkv|所有文件|*.*"</param>
        /// <param name="multiselect">是否允许多选</param>
        /// <returns>选中文件路径数组，取消则返回 null 。</returns>
        ValueOrList<string>? ShowOpenFileDialog(string? title = null, string? filter = null, bool multiselect = false);
    }
}