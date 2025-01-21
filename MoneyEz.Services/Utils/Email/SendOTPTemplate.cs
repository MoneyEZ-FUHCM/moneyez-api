using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Utils.Email
{
    public class SendOTPTemplate
    {
        public static string EmailSendOTP(string email, string otpCode, string fullName)
        {
            #region body

            string body =
                $@"
                <!DOCTYPE html>
                <html lang=""vi"">
                  <head>
                    <meta charset=""UTF-8"" />
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                    <title>Xác Thực Tài Khoản MoneyEz</title>
                  </head>
                  <body
                    style=""
                      font-family: 'Inter', sans-serif;
                      background-color: #f4f4f4;
                      padding: 20px;
                      margin: 0;
                    ""
                  >
                    <div
                      style=""
                        width: 600px;
                        background-color: #24547c;
                        margin: 0 auto;
                        overflow: hidden;
                        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                      ""
                    >
                      <div
                        style=""
                          background-color: #f4a02a;
                          text-align: left;
                          width: 600px;
                          height: 80px;
                          padding: 0 20px;
                          display: table;
                        ""
                      >
                        <div style=""display: table-cell; vertical-align: middle"">
                          <img
                            src=""https://firebasestorage.googleapis.com/v0/b/exe201-9459a.appspot.com/o/Fricks%2FEzMoney_v2.png?alt=media&token=24dc41a4-b872-4424-98f9-d72c82db8b13""
                            alt=""MoneyEz logo""
                            style=""height: 50px""
                          />
                        </div>
                      </div>

                      <div
                        style=""
                          padding: 30px 50px;
                          text-align: left;
                          background-color: #fff;
                          margin-top: 5px;
                        ""
                      >
                        <h2 style=""color: #333"">
                          Xác thực tài khoản
                          <span style=""color: #f4a02a; font-weight: 700"">MoneyEz</span> của bạn
                        </h2>
                        <p>Xin chào, <strong>{fullName}</strong></p>
                        <p>
                          Bạn đã đăng kí email
                          <span style=""font-style: italic; font-weight: 700; color: #24547c""
                            >{email}</span
                          >
                          tại <span style=""color: #f4a02a; font-weight: 600"">MoneyEz</span>
                        </p>
                        <p style=""font-size: 16px; text-align: center; margin-top: 25px"">
                          Mã xác thực tài khoản của bạn là:
                        </p>
                        <div style=""text-align: center; margin: 30px 0"">
                          <span
                            style=""
                              font-size: 30px;
                              font-weight: bolder;
                              color: #fff;
                              background-color: #24547c;
                              padding: 10px 20px;
                              letter-spacing: 8px;
                              text-align: center;
                            ""
                          >
                            {otpCode}
                          </span>
                        </div>
                        <p>
                          Bạn hãy nhập mã xác thực này tại màn hình đăng kí
                          <span style=""color: #f4a02a; font-weight: 700"">MoneyEz</span> để hoàn
                          tất quá trình tạo tài khoản nhé.
                        </p>
                        <p
                          style=""
                            font-size: 16px;
                            font-style: italic;
                            line-height: 22px;
                          ""
                        >
                          <strong style=""color: red;"">Lưu ý:</strong> Mã xác thực chỉ có hiệu lực trong vòng
                          <strong>5 phút</strong>. Vui lòng không cung cấp mã này cho người khác.
                        </p>
                      </div>

                      <div
                        style=""
                          background-color: #24547c;
                          color: #fff;
                          padding: 20px;
                          text-align: center;
                          line-height: 25px;
                        ""
                      >
                        <p
                          style=""
                            margin: 0 0 10px 0;
                            color: orange;
                            font-weight: 700;
                            font-size: 17px;
                          ""
                        >
                          MoneyEz - Trợ thủ tài chính của mọi nhà
                        </p>
                        <a
                          href=""mailto:MoneyEz.customerservice@gmail.com?subject=Contact&body=Dear shop,%0D%0A%0D%0ATôi có vấn đề này...""
                          style=""color: #ffffff; text-decoration: none; font-size: small""
                        >
                          MoneyEz.customerservice@gmail.com
                        </a>

                        <p style=""font-size: small; margin: 5px 0"">0989.998.889</p>
                        <p style=""font-size: small; margin: 0"">
                          Đại học FPT thành phố Hồ Chí Minh
                        </p>
                      </div>
                    </div>
                  </body>
                </html>
                ";
            #endregion body

            return body;
        }

        public static string EmailSendOTPResetPassword(string email, string otpCode, string fullName)
        {

            #region body

            string body =
                $@"
                <!DOCTYPE html>
                <html lang=""vi"">
                  <head>
                    <meta charset=""UTF-8"" />
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                    <title>Xác Thực Tài Khoản MoneyEz</title>
                  </head>
                  <body
                    style=""
                      font-family: 'Inter', sans-serif;
                      background-color: #f4f4f4;
                      padding: 20px;
                      margin: 0;
                    ""
                  >
                    <div
                      style=""
                        width: 600px;
                        background-color: #24547c;
                        margin: 0 auto;
                        overflow: hidden;
                        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                      ""
                    >
                      <div
                        style=""
                          background-color: #f4a02a;
                          text-align: left;
                          width: 600px;
                          height: 80px;
                          padding: 0 20px;
                          display: table;
                        ""
                      >
                        <div style=""display: table-cell; vertical-align: middle"">
                          <img
                            src=""https://firebasestorage.googleapis.com/v0/b/exe201-9459a.appspot.com/o/Fricks%2FEzMoney_v2.png?alt=media&token=24dc41a4-b872-4424-98f9-d72c82db8b13""
                            alt=""MoneyEz logo""
                            style=""height: 50px""
                          />
                        </div>
                      </div>

                      <div
                        style=""
                          padding: 30px 50px;
                          text-align: left;
                          background-color: #fff;
                          margin-top: 5px;
                        ""
                      >
                        <h2 style=""color: #333"">
                          Đặt lại mật khẩu tài khoản
                          <span style=""color: #f4a02a; font-weight: 700"">MoneyEz</span>
                        </h2>
                        <p>Xin chào, <strong>{fullName}</strong></p>
                        <p>
                          Bạn đã đặt lại mật khẩu cho tài khoản
                          <span style=""font-style: italic; font-weight: 700; color: #24547c""
                            >{email}</span>
                        </p>
                        <p style=""font-size: 16px; text-align: center; margin-top: 25px"">
                          Mã xác thực tài khoản của bạn là:
                        </p>
                        <div style=""text-align: center; margin: 30px 0"">
                          <span
                            style=""
                              font-size: 30px;
                              font-weight: bolder;
                              color: #fff;
                              background-color: #24547c;
                              padding: 10px 20px;
                              letter-spacing: 8px;
                              text-align: center;
                            ""
                          >
                            {otpCode}
                          </span>
                        </div>
                        <p>
                          Nếu không phải bạn thực hiện vui lòng đổi mật khẩu để bảo vệ tài khoản 
                          <span style=""color: #f4a02a; font-weight: 700"">MoneyEz</span> của bạn.
                        </p>
                        <p style=""font-size: 16px; font-style: italic; line-height: 22px"">
                          <strong style=""color: red"">Lưu ý:</strong> Mã xác thực chỉ có hiệu lực
                          trong vòng <strong>5 phút</strong>. Vui lòng không cung cấp mã này cho
                          người khác.
                        </p>
                      </div>

                      <div
                        style=""
                          background-color: #24547c;
                          color: #fff;
                          padding: 20px;
                          text-align: center;
                          line-height: 25px;
                        ""
                      >
                        <p
                          style=""
                            margin: 0 0 10px 0;
                            color: orange;
                            font-weight: 700;
                            font-size: 17px;
                          ""
                        >
                          MoneyEz - Trợ thủ tài chính của mọi nhà
                        </p>
                        <a
                          href=""mailto:MoneyEz.customerservice@gmail.com?subject=Contact&body=Dear shop,%0D%0A%0D%0ATôi có vấn đề này...""
                          style=""color: #ffffff; text-decoration: none; font-size: small""
                        >
                          MoneyEz.customerservice@gmail.com
                        </a>

                        <p style=""font-size: small; margin: 5px 0"">0989.998.889</p>
                        <p style=""font-size: small; margin: 0"">
                          Đại học FPT thành phố Hồ Chí Minh
                        </p>
                      </div>
                    </div>
                  </body>
                </html>
                ";
            #endregion body

            return body;
        }
    }
}
