using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IEasing
{
	float ease(float t, float b, float c, float d);
}

public enum EasingType
{
	easelinear,
	
	easeInSine,
	easeOutSine,
	easeInOutSine,
	
	easeInQuad,
	easeOutQuad,
	easeInOutQuad,
	
	easeInCubic,
	easeOutCubic,
	easeInOutCubic,
	
	easeInQuart,
	easeOutQuart,
	easeInOutQuart,
	
	easeInQuint,
	easeOutQuint,
	easeInOutQuint,
	
	easeInExpo,
	easeOutExpo,
	easeInOutExpo,
	
	easeInCirc,
	easeOutCirc,
	easeInOutCirc,
	
	easeInElastic,
	easeOutElastic,
	easeInOutElastic,
	
	easeInBack,
	easeOutBack,
	easeInOutBack,
	
	easeInBounce,
	easeOutBounce,
	easeInOutBounce,
}

public class EasingCore
{
	private static EasingCore _instance;
	public static EasingCore Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = new EasingCore();
				_instance.Init();
			}
			return _instance;
		}
	}

	Dictionary<EasingType, IEasing> _easingMap = new Dictionary<EasingType, IEasing>();

	void Init()
	{
		_easingMap.Add(EasingType.easelinear, new easelinear());
		
		_easingMap.Add(EasingType.easeInSine, new easeInSine());
		_easingMap.Add(EasingType.easeOutSine, new easeOutSine());
		_easingMap.Add(EasingType.easeInOutSine, new easeInOutSine());
		
		_easingMap.Add(EasingType.easeInQuad, new easeInQuad());
		_easingMap.Add(EasingType.easeOutQuad, new easeOutQuad());
		_easingMap.Add(EasingType.easeInOutQuad, new easeInOutQuad());
		
		_easingMap.Add(EasingType.easeInCubic, new easeInCubic());
		_easingMap.Add(EasingType.easeOutCubic, new easeOutCubic());
		_easingMap.Add(EasingType.easeInOutCubic, new easeInOutCubic());
		
		_easingMap.Add(EasingType.easeInQuart, new easeInQuart());
		_easingMap.Add(EasingType.easeOutQuart, new easeOutQuart());
		_easingMap.Add(EasingType.easeInOutQuart, new easeInOutQuart());
		
		_easingMap.Add(EasingType.easeInQuint, new easeInQuint());
		_easingMap.Add(EasingType.easeOutQuint, new easeOutQuint());
		_easingMap.Add(EasingType.easeInOutQuint, new easeInOutQuint());
		
		_easingMap.Add(EasingType.easeInExpo, new easeInExpo());
		_easingMap.Add(EasingType.easeOutExpo, new easeOutExpo());
		_easingMap.Add(EasingType.easeInOutExpo, new easeInOutExpo());
		
		_easingMap.Add(EasingType.easeInCirc, new easeInCirc());
		_easingMap.Add(EasingType.easeOutCirc, new easeOutCirc());
		_easingMap.Add(EasingType.easeInOutCirc, new easeInOutCirc());
		
		_easingMap.Add(EasingType.easeInElastic, new easeInElastic());
		_easingMap.Add(EasingType.easeOutElastic, new easeOutElastic());
		_easingMap.Add(EasingType.easeInOutElastic, new easeInOutElastic());
		
		_easingMap.Add(EasingType.easeInBack, new easeInBack());
		_easingMap.Add(EasingType.easeOutBack, new easeOutBack());
		_easingMap.Add(EasingType.easeInOutBack, new easeInOutBack());
		
		_easingMap.Add(EasingType.easeInBounce, new easeInBounce());
		_easingMap.Add(EasingType.easeOutBounce, new easeOutBounce());
		_easingMap.Add(EasingType.easeInOutBounce, new easeInOutBounce());
	}

	public IEasing GetEasing(EasingType type)
	{
		IEasing easing;
		if(_easingMap.TryGetValue(type, out easing))
		{
			return easing;
		}
		return null;
	}
}

// t: time, b: startValue, c: changeInValue, d: duration
public class easelinear : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return MathUtils.Lerp(b, b+c, t/d);
	}
}

public class easeInSine : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return -c * Mathf.Cos(t/d * (Mathf.PI/2f)) + c + b;
	}
}

public class easeOutSine : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return c * Mathf.Sin(t/d * (Mathf.PI/2f)) + b;
	}
}

public class easeInOutSine : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return -c/2f * (Mathf.Cos(Mathf.PI*t/d) - 1f) + b;
	}
}

public class easeInQuad : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return c*(t/=d)*t + b;
	}
}

public class easeOutQuad : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return -c *(t/=d)*(t-2f) + b;
	}
}

public class easeInOutQuad : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		if ((t/=d/2f) < 1f) return c/2f*t*t + b;
		return -c/2f * ((--t)*(t-2f) - 1f) + b;
	}
}

public class easeInCubic : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return c*(t/=d)*t*t + b;
	}
}

public class easeOutCubic : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return c*((t=t/d-1f)*t*t + 1f) + b;
	}
}

public class easeInOutCubic : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		if ((t/=d/2f) < 1f) return c/2f*t*t*t + b;
		return c/2f*((t-=2f)*t*t + 2f) + b;
	}
}

public class easeInQuart : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return c*(t/=d)*t*t*t + b;
	}
}

public class easeOutQuart : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return -c * ((t=t/d-1f)*t*t*t - 1f) + b;
	}
}

public class easeInOutQuart : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		if ((t/=d/2f) < 1f) return c/2f*t*t*t*t + b;
		return -c/2f * ((t-=2f)*t*t*t - 2f) + b;
	}
}

public class easeInQuint : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return c*(t/=d)*t*t*t*t + b;
	}
}

public class easeOutQuint : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return c*((t=t/d-1f)*t*t*t*t + 1f) + b;
	}
}

public class easeInOutQuint : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		if ((t/=d/2f) < 1f) return c/2f*t*t*t*t*t + b;
		return c/2f*((t-=2f)*t*t*t*t + 2f) + b;
	}
}

public class easeInExpo : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return (t==0f) ? b : c * Mathf.Pow(2f, 10f * (t/d - 1f)) + b;
	}
}

public class easeOutExpo : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return (t==d) ? b+c : c * (-Mathf.Pow(2f, -10f * t/d) + 1f) + b;
	}
}

public class easeInOutExpo : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		if (t==0f) return b;
		if (t==d) return b+c;
		if ((t/=d/2f) < 1f) return c/2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
		return c/2 * (-Mathf.Pow(2f, -10f * --t) + 2f) + b;
	}
}

public class easeInCirc : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return -c * (Mathf.Sqrt(1f - (t/=d)*t) - 1f) + b;
	}
}

public class easeOutCirc : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		return c * Mathf.Sqrt(1f - (t=t/d-1f)*t) + b;
	}
}

public class easeInOutCirc : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		if ((t/=d/2f) < 1f) return -c/2f * (Mathf.Sqrt(1f - t*t) - 1f) + b;
		return c/2 * (Mathf.Sqrt(1 - (t-=2)*t) + 1) + b;
	}
}

public class easeInElastic : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		float s=1.70158f; float p=0f; float a=c;
		if (MathUtils.IsEquals(t, 0f)) return b;  if (MathUtils.IsEquals((t/=d), 1f)) return b+c;  if (!MathUtils.IsZero(p)) p=d*0.3f;
		if (a < Mathf.Abs(c)) { a=c; s = p/4f; }
		else s = p/(2f*Mathf.PI) * Mathf.Asin (c/a);
		return -(a*Mathf.Pow(2f,10f*(t-=1f)) * Mathf.Sin( (t*d-s)*(2f*Mathf.PI)/p )) + b;
	}
}

public class easeOutElastic : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		if (t==0d) return b;  if ((t/=d)==1f) return b+c;  
		float p=d*0.3f;
		float a=c; 
		float s=p/4f;
		return (a*Mathf.Pow(2f,-10f*t) * Mathf.Sin( (t*d-s)*(2f*Mathf.PI)/p ) + c + b);	
	}
}

public class easeInOutElastic : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		if (t==0f) return b;  if ((t/=d/2f)==2f) return b+c; 
		float p=d*(0.3f*1.5f);
		float a=c; 
		float s=p/4f;
		
		if (t < 1f) {
			float postFix =a*Mathf.Pow(2f,10f*(t-=1f)); // postIncrement is evil
			return -0.5f*(postFix* Mathf.Sin( (t*d-s)*(2f*Mathf.PI)/p )) + b;
		} 
		float postFix2 =  a*Mathf.Pow(2f,-10f*(t-=1f)); // postIncrement is evil
		return postFix2 * Mathf.Sin( (t*d-s)*(2f*Mathf.PI)/p )*0.5f + c + b;
	}
}

public class easeInBack : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		float s = 1.70158f;
		return c*(t/=d)*t*((s+1f)*t - s) + b;
	}
}

public class easeOutBack : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		float s = 1.70158f;
		return c*((t=t/d-1f)*t*((s+1f)*t + s) + 1f) + b;
	}
}

public class easeInOutBack : IEasing
{
	public float ease (float t, float b, float c, float d)
	{
		float s = 1.70158f; 
		if ((t/=d/2f) < 1f) return c/2f*(t*t*(((s*=(1.525f))+1f)*t - s)) + b;
		return c/2f*((t-=2f)*t*(((s*=(1.525f))+1f)*t + s) + 2f) + b;
	}
}

public class easeInBounce : IEasing
{
	public float ease(float t, float b, float c, float d) 
	{
		easeOutBounce ease = new easeOutBounce();
		return c -  ease.ease (d-t, 0f, c, d) + b;
	}
}

public class easeOutBounce : IEasing
{
	public float ease(float t, float b, float c, float d) 
	{
		if ((t/=d) < (1f/2.75f)) {
			return c*(7.5625f*t*t) + b;
		} else if (t < (2f/2.75f)) {
			return c*(7.5625f*(t-=(1.5f/2.75f))*t + 0.75f) + b;
		} else if (t < (2.5f/2.75f)) {
			return c*(7.5625f*(t-=(2.25f/2.75f))*t + 0.9375f) + b;
		} else {
			return c*(7.5625f*(t-=(2.625f/2.75f))*t + 0.984375f) + b;
		}
	}
}

public class easeInOutBounce : IEasing
{
	public float ease(float t, float b, float c, float d) 
	{
		easeInBounce easein = new easeInBounce();
		easeOutBounce easeout = new easeOutBounce();
		
		if (t < d/2f) return easein.ease (t*2f, 0f, c, d) * 0.5f + b;
		return easeout.ease (t*2f-d, 0f, c, d) * 0.5f + c*0.5f + b;
	}
}

