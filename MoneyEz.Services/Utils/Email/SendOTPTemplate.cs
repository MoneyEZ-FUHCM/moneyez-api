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
                    "">
                    <div
                      style=""
                        width: 600px;
                        background-color: #637c92;
                        margin: 0 auto;
                        overflow: hidden;
                        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                      "">
                      <div
                        style=""
                          display: flex;
                          background-color: #7bc5b2;
                          text-align: left;
                          overflow: hidden;
                          width: 600px;
                          height: 100px;
                          align-items: center;
                          padding: 0 20px;
                        "">
                        <img
                          src=""https://firebasestorage.googleapis.com/v0/b/swd392-d2c4e.appspot.com/o/Ezmoney%2FEzMoney_v2%20(1).png?alt=media&token=95d197f2-5ddd-4efc-b25f-3ad1db18f147""
                          alt=""Fricks logo""
                          style=""height: 50px"" />

                        <div
                          style=""
                            width: 40px;
                            height: 40px;
                            background-color: #24547cbe;
                            transform: rotate(30deg);
                            margin-left: -200px;
                            margin-top: 80px;
                          ""></div>

                        <div
                          style=""
                            width: 20px;
                            height: 20px;
                            background-color: #24547cbe;
                            border-radius: 50%;
                            margin-left: 100px;
                            margin-top: -80px;
                          ""></div>

                        <div
                          style=""
                            width: 15px;
                            height: 15px;
                            background-color: #24547cbe;
                            border-radius: 50%;
                            margin-left: 50px;
                            margin-top: 20px;
                            opacity: 0;
                          ""></div>

                        <div
                          style=""
                            width: 40px;
                            height: 40px;
                            background-color: #24547cbe;
                            border-radius: 50%;
                            margin-top: 95px;
                          ""></div>

                        <div
                          style=""
                            width: 40px;
                            height: 40px;
                            background-color: #24547cbe;
                            transform: rotate(30deg);
                            margin-left: 80px;
                            margin-top: -90px;
                          ""></div>

                        <div
                          style=""
                            width: 30px;
                            height: 30px;
                            background-color: #24547cbe;
                            transform: rotate(60deg);
                            margin-left: 10px;
                            margin-top: -60px;
                            opacity: 0;
                          ""></div>

                        <div
                          style=""
                            width: 30px;
                            height: 30px;
                            background-color: #24547cbe;
                            transform: rotate(65deg);
                            margin-left: 100px;
                            margin-top: 100px;
                          ""></div>
                        <div
                          style=""
                            width: 15px;
                            height: 15px;
                            background-color: #24547cbe;
                            border-radius: 50%;
                            margin-top: -10px;
                            margin-right: 50px;
                          ""></div>

                        <div
                          style=""
                            width: 35px;
                            height: 35px;
                            border-radius: 50%;
                            background-color: #24547cbe;
                            transform: rotate(65deg);
                            margin-left: -50px;
                            margin-top: -80px;
                          ""></div>
                      </div>

                      <div
                        style=""
                          padding: 30px 50px;
                          text-align: left;
                          background-color: #fff;
                          margin-top: 5px;
                        "">
                        <h2 style=""color: #333"">
                          Xác thực tài khoản
                          <span style=""color: #609084; font-weight: 700"">EzMoney</span> của bạn
                        </h2>
                        <p>Xin chào, <strong>{fullName}</strong></p>
                        <p>
                          Bạn đã đăng kí email
                          <span
                            style=""font-style: italic; font-weight: 700; color: #609084"">{email}</span>
                          tại <span style=""color: #609084; font-weight: 600"">EzMoney</span>
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
                              background-color: #609084;
                              padding: 10px 20px;
                              letter-spacing: 8px;
                              text-align: center;
                            "">
                            {otpCode}
                          </span>
                        </div>
                        <p>
                          Bạn hãy nhập mã xác thực này tại màn hình đăng kí
                          <span style=""color: #609084; font-weight: 700"">EzMoney </span> để hoàn
                          tất quá trình tạo tài khoản nhé.
                        </p>
                        <p
                          style=""
                            color: red;
                            font-size: 16px;
                            font-style: italic;
                            line-height: 22px;
                          "">
                          <strong>Lưu ý:</strong> Mã xác thực chỉ có hiệu lực trong vòng
                          <strong>5 phút</strong>. Không cung cấp mã này cho người khác.
                        </p>
                      </div>

                      <div
                        style=""
                          background-color: #609084;
                          color: #fff;
                          padding: 20px;
                          text-align: center;
                          line-height: 25px;
                        "">
                        <p
                          style=""
                            margin: 0 0 10px 0;
                            color: #BAD8B6;
                            font-weight: 700;
                            font-size: 17px;
                          "">
                          EzMoney - Trợ thủ tài chính của mọi nhà
                        </p>
                        <p style=""font-size: small; margin: 5px 0; color: white !important""></p>
                        <a
                          href=""mailto:fricks.customerservice@gmail.com?subject=Contact&body=Dear shop,%0D%0A%0D%0ATôi có vấn đề này...""
                          style=""color: #ffffff; text-decoration: none"">
                          ezmoney.customerservice@gmail.com
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
