using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Utils.Email
{
    public class SendInviteGroupMember
    {
        public static string EmailSendInviteGroupMember(string fromMember, string toMember, string groupName, string description, string linkInvite)
        {
            #region style
            string style = $@"
                <style>
                    * {{
                        box-sizing: border-box;
                        margin: 0;
                        padding: 0;
                    }}
                    body {{
                        font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                        line-height: 1.6;
                        background-color: #E1EACD;
                        color: #2c3e50;
                        -webkit-font-smoothing: antialiased;
                        padding: 20px 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        background-color: white;
                        border-radius: 16px;
                        box-shadow: 0 15px 35px rgba(96, 144, 132, 0.15);
                        overflow: hidden;
                        border: 1px solid #BAD8B6;
                    }}
                    .header {{
                        background: linear-gradient(135deg, #609084 0%, #4a7a6e 100%);
                        color: white;
                        padding: 30px 25px;
                        text-align: center;
                        position: relative;
                        overflow: hidden;
                    }}
                    .header::before {{
                        content: '';
                        position: absolute;
                        top: -50%;
                        left: -50%;
                        width: 200%;
                        height: 200%;
                        background: rgba(255,255,255,0.1);
                        transform: rotate(-45deg);
                    }}
                    .header h1 {{
                        font-size: 26px;
                        font-weight: 600;
                        position: relative;
                        z-index: 1;
                    }}
                    .content {{
                        padding: 35px 25px;
                        background-color: #EBEFD6;
                    }}
                    .content h2 {{
                        color: #609084;
                        margin-bottom: 20px;
                        font-weight: 600;
                    }}
                    .group-details {{
                        background-color: white;
                        border: 1px solid #BAD8B6;
                        border-radius: 12px;
                        padding: 20px;
                        margin-top: 25px;
                        box-shadow: 0 8px 15px rgba(96, 144, 132, 0.05);
                    }}
                    .group-details h3 {{
                        color: #609084;
                        margin-bottom: 15px;
                        font-weight: 600;
                    }}
                    .invitation-link {{
                        display: block;
                        width: 100%;
                        background: linear-gradient(135deg, #609084 0%, #4a7a6e 100%);
                        color: white;
                        text-align: center;
                        padding: 15px 20px;
                        text-decoration: none;
                        border-radius: 10px;
                        margin-top: 25px;
                        font-weight: 600;
                        transition: transform 0.3s ease, box-shadow 0.3s ease;
                        box-shadow: 0 10px 20px rgba(96, 144, 132, 0.2);
                    }}
                    .invitation-link:hover {{
                        transform: translateY(-3px);
                        box-shadow: 0 15px 25px rgba(96, 144, 132, 0.3);
                    }}
                    .footer {{
                        background-color: #BAD8B6;
                        text-align: center;
                        padding: 25px;
                        font-size: 0.9em;
                        color: #2c3e50;
                    }}
                    .footer a {{
                        color: #609084;
                        text-decoration: none;
                        font-weight: 600;
                    }}
                    .footer a:hover {{
                        text-decoration: underline;
                    }}
                    .link-section {{
                        background-color: #f9f9f9;
                        border: 1px dashed #BAD8B6;
                        border-radius: 8px;
                        padding: 15px;
                        margin-top: 20px;
                        text-align: center;
                    }}

                    @media screen and (max-width: 600px) {{
                        body {{
                            padding: 0;
                        }}
                        .container {{
                            width: 100%;
                            border-radius: 0;
                            box-shadow: none;
                            border: none;
                        }}
                        .content {{
                            padding: 25px 15px;
                        }}
                        .header {{
                            padding: 20px 15px;
                        }}
                        .header h1 {{
                            font-size: 22px;
                        }}
                    }}
                </style>
             ";
            #endregion

            #region body
            string body = $@"

            < !DOCTYPE html>
            <html lang=""vi"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Lời Mời Tham Gia Nhóm</title>
                <link href=""https://fonts.googleapis.com/css2?family=Inter:wght@300;400;600&display=swap"" rel=""stylesheet"">
                {style}
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>Lời mời tham gia nhóm trên MoneyEz</h1>
                    </div>
                    <div class=""content"">
                        <h2>Xin chào {toMember}!</h2>
                        <p>Bạn đã được {fromMember} mời tham gia nhóm <strong>{groupName}</strong>. Đây là cơ hội tuyệt vời để kết nối và cộng tác.</p>
            
                        <div class=""group-details"">
                            <h3>Lời mời</h3>
                            <p>{description}</p>
                        </div>
            
                        <a href=""{linkInvite}"" class=""invitation-link"">Tham Gia Ngay</a>
            
                        <div class=""link-section"">
                            <p style=""font-size: 0.9em;"">Nếu nút không hoạt động, sao chép liên kết sau:</p>
                            <small style=""color: #609084; word-break: break-all;"">{linkInvite}</small>
                        </div>
                    </div>
                    <div class=""footer"">
                        <p>© 2025 <a target=""_blank"" href=""https://easymoney.anttravel.online/moneyez-web/"">MoneyEz</a>. Mọi quyền được bảo lưu.</p>
                        <p>Nếu bạn không muốn nhận email này, vui lòng bỏ qua</a>.</p>
                    </div>
                </div>
            </body>
            </html>


            ";
            #endregion

            return body;
        }
    }
}
