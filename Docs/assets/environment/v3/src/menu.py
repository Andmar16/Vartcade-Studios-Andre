"""v3.2 main menu mockup on the final street (M-A), same layout as MainMenuController."""
import sys
sys.path.insert(0, "/home/claude/envgen")
from lib import page, caption, write, GOLD, CHAR, CREAM
INK = "#1e1411"
css = f"""
.wrap{{position:relative;width:1600px;height:900px;overflow:hidden;flex:none}}
.screen{{position:relative;width:1920px;height:1080px;transform:scale(.8333);transform-origin:0 0;overflow:hidden;background:#000}}
.full{{position:absolute;inset:0;width:100%;height:100%;display:block}}
.clouds{{animation:drift 40s linear infinite alternate}}@keyframes drift{{from{{transform:translateX(-30px)}}to{{transform:translateX(60px)}}}}
.laundry{{transform-origin:83% 34%;animation:flap 1.9s ease-in-out infinite alternate}}@keyframes flap{{from{{transform:skewX(-1.2deg)}}to{{transform:skewX(1.2deg)}}}}
.crows{{animation:crows 14s linear infinite}}@keyframes crows{{from{{transform:translate(0,0)}}to{{transform:translate(-320px,-40px)}}}}
.shade{{position:absolute;inset:0;background:linear-gradient(90deg,rgba(14,8,6,.78) 0,rgba(14,8,6,.55) 34%,rgba(14,8,6,0) 52%)}}
.warm{{position:absolute;inset:0;background:rgba(255,168,92,.12);mix-blend-mode:soft-light}}
.logo{{position:absolute;left:120px;top:110px;font:400 150px/0.92 Anton,sans-serif;color:#f4d65a;letter-spacing:2px;
 -webkit-text-stroke:5px #b91c14;paint-order:stroke fill;text-shadow:5px 6px 0 #3a0a08,9px 11px 0 #1a0605;transform:rotate(-2deg);transform-origin:0 0}}
.logo span{{display:block;font-size:123px;color:{GOLD}}}
.logo{{animation:bob 4s ease-in-out infinite alternate}}@keyframes bob{{from{{translate:0 0}}to{{translate:0 -4px}}}}
.tagline{{position:absolute;left:124px;top:425px;font:italic 400 30px Roboto,sans-serif;color:{CREAM};text-shadow:0 2px 0 #000}}
.btn{{position:absolute;left:120px;width:492px;box-sizing:border-box;border:3px solid {INK};border-radius:14px;outline:3px solid rgba(239,230,210,.9);
 display:flex;align-items:center;padding-left:28px;font:400 38px Anton,sans-serif;letter-spacing:1px;box-shadow:0 7px 0 rgba(0,0,0,.55),8px 14px 0 rgba(0,0,0,.25)}}
.btn:hover{{translate:0 -3px}}
.small{{position:absolute;width:156px;height:70px;box-sizing:border-box;border:3px solid {INK};border-radius:12px;outline:3px solid rgba(239,230,210,.9);
 display:flex;align-items:center;justify-content:center;font:400 26px Anton,sans-serif;background:#13191d;color:{CREAM};box-shadow:0 6px 0 rgba(0,0,0,.55)}}
.new{{margin-left:auto;margin-right:24px;padding:4px 12px;border-radius:8px;background:{CHAR};color:#f4d65a;font-size:22px;border:2px solid {INK};transform:rotate(4deg)}}
.chip{{position:absolute;right:60px;top:60px;width:410px;height:90px;box-sizing:border-box;border-radius:14px;background:rgba(19,25,29,.88);border:3px solid {INK};outline:3px solid rgba(239,230,210,.85);
 display:flex;align-items:center;gap:14px;padding:0 14px;color:{CREAM}}}
.chip img{{width:64px;height:64px;object-fit:cover;object-position:50% 8%;border-radius:50%;background:#3e2a1f;border:3px solid {GOLD}}}
.chip b{{font:400 28px Anton,sans-serif;display:block}}.chip small{{font:400 18px Roboto,sans-serif;color:#cfc2ad}}
.pup{{position:absolute;right:30px;bottom:-6px;height:470px;filter:drop-shadow(10px 12px 6px rgba(0,0,0,.45));animation:breathe 3.4s ease-in-out infinite alternate;transform-origin:50% 100%}}
@keyframes breathe{{from{{transform:scaleY(1)}}to{{transform:scaleY(1.012)}}}}
.pupshadow{{display:none}}
.footer{{position:absolute;right:420px;bottom:30px;font:400 20px Roboto,sans-serif;color:#cfc2ad}}
@media (prefers-reduced-motion: reduce){{*{{animation:none!important}}}}
"""
def B(y, h, text, bg, fg, size=38, extra=""):
    return f'<div class="btn" style="top:{y}px;height:{h}px;background:{bg};color:{fg};font-size:{size}px">{text}{extra}</div>'
y = 510
btns = B(y, 92, "PLAY ONLINE", "#a8392f", CREAM, 46); y += 92 + 22
btns += B(y, 80, "PLAY VS BOTS", "#e9dcc2", CHAR); y += 80 + 22
btns += B(y, 80, "TUTORIAL", GOLD, CHAR, extra='<span class="new">NEW</span>'); y += 80 + 26
for i, t in enumerate(["CARDS", "PROFILE", "SETTINGS"]):
    btns += f'<div class="small" style="left:{120 + i*168}px;top:{y}px">{t}</div>'
y += 70 + 26
btns += f'<div class="small" style="left:120px;top:{y}px;width:180px;background:rgba(0,0,0,.35)">QUIT</div>'
screen = f'''<div class="wrap"><div class="screen">
<img class="full" src="./v3/shero_d1_base.jpg" alt="Main menu street at sundown"><img class="full clouds" src="./v3/shero_d1_clouds.png" alt="">
<img class="full crows" src="./v3/shero_d1_crows.png" alt=""><img class="full laundry" src="./v3/shero_d1_laundry.png" alt="">
<div class="shade"></div><div class="warm"></div>
<div class="logo">TEN DAYS<span>TO SUNDOWN</span></div>
<div class="tagline">The bunker has room for two. Make sure one of them is you.</div>
{btns}
<div class="chip"><img src="./v3/cast/c1.png" alt=""><div><b>ANDRE</b><small>12 wins · streak 3</small></div></div>
<div class="pupshadow"></div><img class="pup" src="./v3/cast/c1.png" alt="Your avatar">
<div class="footer">v0.9 · Vartcade Studios</div>
</div></div>'''
inner = screen + caption("MENU", "MAIN MENU ON THE FINAL STREET", "Same layout as the game's MainMenuController: logo top-left, buttons in the left half (a dark wash keeps them readable over the store), profile chip top-right, your avatar bottom-right. Clouds, crows and laundry move; the logo bobs.", "Paper edges on the buttons\nmatch the cut-out scenes")
write("V3_Menu.dc.html", page("v3.2: Main menu", 1600, 980, inner, css=css))
print("menu ok")
