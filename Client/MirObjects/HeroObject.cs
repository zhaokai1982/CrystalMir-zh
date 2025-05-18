// 引入客户端控件命名空间
using Client.MirControls;
// 引入客户端场景对话框命名空间
using Client.MirScenes.Dialogs;
// 为 ServerPackets 命名空间创建别名 S
using S = ServerPackets;

namespace Client.MirObjects
{
    /// <summary>
    /// 英雄对象类，继承自玩家对象类，用于表示游戏中的英雄角色。
    /// </summary>
    public class HeroObject : PlayerObject
    {
        /// <summary>
        /// 重写对象类型属性，返回该对象的类型为英雄。
        /// </summary>
        public override ObjectType Race
        {
            get { return ObjectType.Hero; }
        }

        // 英雄所有者的名称
        public string OwnerName;
        // 显示英雄所有者名称的标签控件
        public MirLabel OwnerLabel;

        /// <summary>
        /// 判断是否应该绘制英雄的生命值条。
        /// 如果英雄的所有者在组队列表中或者所有者就是当前用户，则绘制生命值条。
        /// </summary>
        /// <returns>如果应该绘制生命值条返回 true，否则返回 false。</returns>
        public override bool ShouldDrawHealth()
        {
            if (GroupDialog.GroupList.Contains(OwnerName) || OwnerName == User.Name)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 英雄对象的构造函数，使用指定的对象 ID 初始化英雄对象。
        /// </summary>
        /// <param name="objectID">英雄对象的唯一标识符。</param>
        public HeroObject(uint objectID) : base(objectID)
        {
        }

        /// <summary>
        /// 加载英雄对象的信息。
        /// </summary>
        /// <param name="info">包含英雄对象信息的数据包。</param>
        public void Load(S.ObjectHero info)
        {
            // 先加载玩家对象的信息
            Load((S.ObjectPlayer)info);
            // 设置英雄所有者的名称
            OwnerName = info.OwnerName;

            // 如果当前信息对应的对象 ID 与当前英雄的对象 ID 相同，则更新英雄的当前位置
            if (info.ObjectID == Hero?.ObjectID)
                Hero.CurrentLocation = info.Location;
        }

        /// <summary>
        /// 创建显示英雄名称和所有者名称的标签控件。
        /// </summary>
        public override void CreateLabel()
        {
            // 调用基类的创建标签方法
            base.CreateLabel();

            // 初始化所有者标签为 null
            OwnerLabel = null;
            // 构建显示所有者信息的文本
            string ownerText = $"{OwnerName}'的英雄";

            // 遍历标签列表，查找匹配的所有者标签
            for (int i = 0; i < LabelList.Count; i++)
            {
                if (LabelList[i].Text != ownerText || LabelList[i].ForeColour != NameColour) continue;
                OwnerLabel = LabelList[i];
                break;
            }

            // 如果找到了有效的所有者标签且未被释放，则直接返回
            if (OwnerLabel != null && !OwnerLabel.IsDisposed) return;

            // 创建新的所有者标签
            OwnerLabel = new MirLabel
            {
                AutoSize = true,
                BackColour = Color.Transparent,
                ForeColour = NameColour,
                OutLine = true,
                OutLineColour = Color.Black,
                Text = ownerText,
            };
            // 当标签被释放时，从标签列表中移除该标签
            OwnerLabel.Disposing += (o, e) => LabelList.Remove(OwnerLabel);
            // 将新的所有者标签添加到标签列表中
            LabelList.Add(OwnerLabel);
        }

        /// <summary>
        /// 绘制英雄的名称和所有者名称标签。
        /// </summary>
        public override void DrawName()
        {
            // 确保标签已经创建
            CreateLabel();

            // 如果名称标签或所有者标签为空，则不进行绘制
            if (NameLabel == null || OwnerLabel == null) return;

            // 设置名称标签的位置
            NameLabel.Location = new Point(DisplayRectangle.X + (50 - NameLabel.Size.Width) / 2, DisplayRectangle.Y - (42 - NameLabel.Size.Height / 2) + (Dead ? 35 : 8));
            // 绘制名称标签
            NameLabel.Draw();

            // 设置所有者标签的位置
            OwnerLabel.Location = new Point(DisplayRectangle.X + (50 - OwnerLabel.Size.Width) / 2, NameLabel.Location.Y + NameLabel.Size.Height - 1);
            // 绘制所有者标签
            OwnerLabel.Draw();
        }
    }
}
